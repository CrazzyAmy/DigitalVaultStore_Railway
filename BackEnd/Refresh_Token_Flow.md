# Refresh Token 流程說明文件

**適用對象：** 後端 / 前端開發人員  
**最後更新：** 2026-03-31  
**版本：** 1.0

---

## 1. 概述

本文件說明系統的 Token 生命週期管理策略，用於使用者身份驗證。系統使用兩種 Token 來兼顧安全性與使用者體驗：

| Token | 有效期限 | 用途 |
|---|---|---|
| **Login Token** | 2 天 | 短效 Token，用於驗證 API 請求 |
| **Refresh Token** | 180 天 | 長效 Token，用於靜默更新 Login Token |

---

## 2. Token 定義

### Login Token
- 使用者成功登入後發放
- 附加於每個需驗證的 API 請求中（通常放在 `Authorization` Header，格式為 Bearer Token）
- 有效期限為 **2 天**

### Refresh Token
- 與 Login Token 一同在登入時發放
- 安全儲存於用戶端（例如 HttpOnly Cookie 或安全儲存空間）
- 用於取得新的 Login Token，無需使用者重新登入
- 有效期限為 **180 天**

---

## 3. 流程摘要

```
使用者登入
    └─> 發放 Login Token + Refresh Token
            │
            ├─ [兩天內]
            │       └─> API 請求成功
            │           伺服器靜默重發新的 Login Token + Refresh Token
            │
            ├─ [兩天後，180 天內]
            │       └─> Login Token 已過期
            │           用戶端將 Refresh Token 送至驗證端點
            │           伺服器將舊的 Login Token + Refresh Token 記入黑名單
            │           伺服器發放新的 Login Token + Refresh Token
            │           使用者自動登入（無感切換）
            │
            └─ [180 天後]
                    └─> Refresh Token 已過期
                        使用者須手動重新登入
                        伺服器重發新的 Login Token + Refresh Token
```

---

## 4. 步驟說明

### 步驟 1 — 使用者登入
- 使用者提交帳號密碼
- 伺服器驗證帳號密碼
- 伺服器產生並回傳：
  - `login_token`（2 天後過期）
  - `refresh_token`（180 天後過期）

### 步驟 2 — 兩天內（Token 有效）
- 用戶端在 API 請求 Header 中附上 `login_token`
- 伺服器驗證 Token 後處理請求
- **靜默更新：** 伺服器主動重發新的 `login_token` 與 `refresh_token`，延長登入狀態

### 步驟 3 — 兩天後、180 天內（Login Token 過期）
- 用戶端偵測到 `login_token` 已過期（例如收到 401 Unauthorized 回應）
- 用戶端將 `refresh_token` 送至 Token 更新端點
- 伺服器執行以下操作：
  1. 驗證 `refresh_token` 是否有效
  2. 將舊的 `login_token` 與 `refresh_token` **記入黑名單**
  3. 發放新的 `login_token` 與 `refresh_token`
- 用戶端儲存新 Token 並重試原始請求
- 使用者無感，自動完成登入

### 步驟 4 — 180 天後（Refresh Token 過期）
- 用戶端將 `refresh_token` 送至 Token 更新端點
- 伺服器拒絕請求（Token 已過期或已列入黑名單）
- 用戶端清除已儲存的 Token，並將使用者導向登入頁面
- 使用者手動登入 → 回到步驟 1

---

## 5. API 端點

### 登入
```
POST /auth/login
Body: { "username": "...", "password": "..." }
Response: { "login_token": "...", "refresh_token": "..." }
```

### Token 更新
```
POST /auth/refresh
Body: { "refresh_token": "..." }
Response（成功）: { "login_token": "...", "refresh_token": "..." }
Response（失敗）: { "error": "refresh_token_expired" }
```

### 登出
```
POST /auth/logout
Headers: Authorization: Bearer <login_token>
Body: { "refresh_token": "..." }
Action: 立即將兩個 Token 記入黑名單
```

---

## 6. 錯誤處理

| 情境 | HTTP 狀態碼 | 錯誤代碼 | 用戶端處理方式 |
|---|---|---|---|
| Login Token 過期 | 401 | `login_token_expired` | 呼叫 `/auth/refresh` |
| Refresh Token 過期 | 401 | `refresh_token_expired` | 導向登入頁面 |
| Refresh Token 已列入黑名單 | 401 | `refresh_token_revoked` | 導向登入頁面 |
| Token 格式錯誤 | 400 | `invalid_token` | 導向登入頁面 |
| 更新時伺服器錯誤 | 500 | `server_error` | 指數退避後重試 |

---

## 7. 安全性注意事項

- **Token 輪換：** 每次更新時，舊的 Token 對會立即失效並發放新的，縮短 Token 被竊用的風險窗口。
- **黑名單機制：** Token 在更新或登出時立即列入黑名單，伺服器須在每次請求時檢查黑名單。
- **儲存方式：** 禁止將 Token 儲存於 `localStorage`，應使用 `HttpOnly` Cookie 或平台原生安全儲存，以防止 XSS 攻擊。
- **傳輸安全：** 所有 Token 交換皆須透過 **HTTPS** 進行。
- **Refresh Token 重複使用偵測：** 若已列入黑名單的 Refresh Token 再次被使用，視為可能的 Token 竊取事件，應立即撤銷該使用者的所有登入狀態。
- **登出處理：** 登出時，伺服器需立即將兩個 Token 均列入黑名單。

---

## 8. Token 生命週期示意圖

```
第 0 天        第 2 天              第 180 天
  |─────────────|─────────────────────|──────────>
  ^             ^                     ^
  登入          Login Token 過期       Refresh Token 過期
  （兩者同時    → 靜默更新             → 強制重新登入
  發放）          或使用 Refresh Token
```

---

## 9. 完整呼叫流程

以下為 `POST /auth/refresh` 的實際執行流程：

```
Client 呼叫 POST /auth/refresh
Body: { refreshToken: "...", oldLoginToken: "eyJhbGci..." }
│
▼
TokenBlacklistMiddleware
← 此請求本身不帶舊 JWT，直接通過
│
▼
AuthService.RefreshAsync()
├─ 查詢 DB，以 refreshToken 找到對應 User
├─ 檢查 RefreshTokenExpiry 是否過期
├─ Blacklist(oldLoginToken)       ← 舊 JWT 從此失效
├─ GenerateToken()                ← 產生新的 Token Pair
└─ 更新 DB 中的 RefreshToken
│
▼
回傳新的 { token, refreshToken }

後續 Client 使用舊 JWT 呼叫任何 API：
└─ TokenBlacklistMiddleware 攔截 → 401 token_revoked
```

### 關鍵設計說明

| 環節 | 說明 |
|---|---|
| `oldLoginToken` 放在 Body | 更新請求本身不在 Header 帶舊 JWT，避免被 Middleware 直接攔截 |
| `Blacklist(oldLoginToken)` | 更新成功後立即將舊 JWT 寫入黑名單，防止舊 Token 繼續被使用 |
| `TokenBlacklistMiddleware` | 每次 API 請求均會檢查黑名單，命中則回傳 `401 token_revoked` |
| `GenerateToken()` | 同時產生新的 `loginToken` 與 `refreshToken`（Token 輪換） |
| 更新 DB RefreshToken | 舊的 Refresh Token 同步作廢，防止重複使用 |

---

## 10. 修訂記錄

| 版本 | 日期 | 作者 | 備註 |
|---|---|---|---|
| 1.0 | 2026-03-31 | — | 初稿 |
