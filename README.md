# Digital Vault 🛍️
![Tests](https://github.com/DrinkAsWater/DigitalVault2026/actions/workflows/test.yml/badge.svg)

> 數位商品電商平台 — 即購即用，無需等待配送

---

## 專案結構

```
DigitalVault2026/
├── backend/
│   └── DigitalProject/       .NET 8 WebAPI
└── frontend/
    └── digital-vault/        React 19 + Vite
```

---

## 技術架構

### 後端
- **框架**：.NET 8 WebAPI
- **資料庫**：SQL Server + Entity Framework Core
- **認證**：JWT Bearer + Google OAuth + HttpOnly Cookie + Token 黑名單
- **權限**：RBAC 角色控制（Policy）
- **即時通知**：SignalR（後台訂單推播）
- **快取**：Redis（商品列表 / 分類）
- **測試**：xUnit + WebApplicationFactory（58 個 Integration Tests）
- **CI/CD**：GitHub Actions（每次 Push 自動執行）

### 前端
- **框架**：React 19 + Vite
- **路由**：React Router v7（懶加載 + 巢狀路由）
- **狀態管理**：Context API（UIContext / AuthContext / CartContext）
- **HTTP**：Axios（含 interceptor 自動刷新 Token）
- **樣式**：純 CSS（CSS Variables，支援 768px / 1920px / 2560px 響應式）

---

## 環境需求

- Node.js 18+
- .NET 8 SDK
- SQL Server
- Redis

---

## 本地開發設定

### 後端

```bash
cd backend/DigitalProject
dotnet restore
dotnet run
```

後端運行於 `https://localhost:7124`

在 `appsettings.Development.json` 設定（不提交 Git）：

```json
{
  "ConnectionStrings": {
    "DbContext": "你的 SQL Server 連線字串"
  },
  "JwtTokenSettings": {
    "Issuer": "DigitalProject",
    "Audience": "DigitalProjectUsers",
    "IssuerSigningKey": "你的密鑰（至少 32 字元）",
    "ExpirationMinutes": "2880"
  },
  "Authentication": {
    "Google": {
      "ClientId": "你的 Google Client ID",
      "ClientSecret": "你的 Google Client Secret"
    }
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### 前端

```bash
cd frontend/digital-vault
npm install
npm run dev
```

前端運行於 `http://localhost:5173`

在前端根目錄建立 `.env`（不提交 Git）：

```env
VITE_API_URL=http://localhost:5173/api
```

Vite Proxy 已設定於 `vite.config.js`，自動轉發 `/api` 至後端。

---

## 設定管理員帳號

```sql
UPDATE UserRoles
SET RoleId = (SELECT Id FROM Roles WHERE Code = 'admin')
WHERE UserId = (SELECT Id FROM Users WHERE Email = '你的Email');
```

---

## RBAC 角色權限

| 角色 | 說明 | 可存取後台 |
|---|---|---|
| `user` | 一般使用者 | ❌ |
| `manager` | 商品管理員 | 商品、分類管理 |
| `support` | 客服人員 | 訂單、付款、評論管理 |
| `admin` | 系統管理員 | 全部 |

> 多角色用逗號分隔，例如 `admin,manager`

---

## API 文件

後端啟動後可至 Swagger 查看完整 API：

```
https://localhost:7124/swagger
```

### 前台端點

| 方法 | 路由 | 說明 | 需要登入 |
|---|---|---|---|
| POST | `/api/auth/register` | 註冊 | ❌ |
| POST | `/api/auth/login` | 登入 | ❌ |
| POST | `/api/auth/logout` | 登出 | ✅ |
| POST | `/api/auth/refresh` | 刷新 Token | ❌ |
| GET | `/api/auth/google` | Google 登入 | ❌ |
| GET | `/api/category` | 取得分類列表 | ❌ |
| GET | `/api/product` | 取得商品列表（搜尋 / 排序 / 分類 / 分頁） | ❌ |
| GET | `/api/product/:id` | 取得商品詳情 | ❌ |
| GET | `/api/order` | 取得我的訂單 | ✅ |
| PUT | `/api/order/:id/cancel` | 取消訂單 | ✅ |
| GET | `/api/order/:id/download` | 取得下載連結 | ✅ |
| POST | `/api/payment/checkout` | 結帳（建立訂單 + 付款） | ✅ |
| PUT | `/api/payment/:id/cvs-confirm` | 超商繳費確認 | ✅ |
| GET | `/api/review/product/:productId` | 取得商品評論 | ❌ |
| POST | `/api/review` | 新增評論（需購買記錄） | ✅ |
| PUT | `/api/review/:id` | 修改評論 | ✅ |
| DELETE | `/api/review/:id` | 刪除評論 | ✅ |
| GET | `/api/user/purchases` | 取得已購商品 | ✅ |
| PUT | `/api/user/avatar` | 上傳頭像 | ✅ |

### 後台端點

| 方法 | 路由 | 說明 | 權限 |
|---|---|---|---|
| GET | `/api/admin/stats` | 後台統計數據 | admin |
| GET/POST | `/api/admin/product` | 商品列表 / 新增 | manager |
| PUT/DELETE | `/api/admin/product/:id` | 編輯 / 下架 | manager |
| GET/POST | `/api/admin/category` | 分類列表 / 新增 | manager |
| GET | `/api/admin/order` | 所有訂單 | support |
| GET | `/api/admin/payment` | 付款記錄 | support |
| PUT | `/api/admin/payment/:id/void` | 作廢付款 | support |
| GET | `/api/admin/user` | 用戶列表 | admin |
| PUT | `/api/admin/user/:id/role` | 更新用戶角色 | admin |
| GET/DELETE | `/api/admin/review` | 評論管理 | support |

---

## 付款流程

```
信用卡：
選擇信用卡 → 填寫卡片資訊 → 付款成功 → 建立訂單（status=1）

超商：
選擇超商 → 建立訂單（status=0）→ 取得繳費代碼 → 超商繳費 → status=1
```

### 信用卡測試

- 卡號末四碼非 `0000` → 付款成功
- 卡號末四碼為 `0000` → 付款失敗

---

## 測試

```bash
cd backend/DigitalProject
dotnet test
```

共 58 個 Integration Tests，覆蓋 Auth / Product / Order / Payment / Review / Admin 所有核心 API。

---

## 注意事項

- `.env` 和 `appsettings.Development.json` 不提交 Git
- Google OAuth Callback URL 需在 Google Cloud Console 設定為 `https://localhost:7124/signin-google`
- 頭像儲存於後端 `wwwroot/uploads/avatars/`，部署時需確認資料夾有寫入權限
