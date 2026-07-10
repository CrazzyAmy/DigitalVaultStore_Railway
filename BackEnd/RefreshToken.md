## 完整呼叫流程
```
Client 呼叫 POST /auth/refresh
  Body: { refreshToken: "...", oldLoginToken: "eyJhbGci..." }
        │
        ▼
TokenBlacklistMiddleware     ← 這次請求本身不帶舊 JWT，所以直接通過
        │
        ▼
AuthService.RefreshAsync()
  ├─ 查 DB 找 User（用 refreshToken）
  ├─ 檢查 RefreshTokenExpiry
  ├─ Blacklist(oldLoginToken)  ← 舊 JWT 從此失效
  ├─ GenerateToken()           ← 產生新 token pair
  └─ 更新 DB 的 RefreshToken
        │
        ▼
回傳新的 { token, refreshToken }

之後 Client 用舊 JWT 打任何 API
  └─ TokenBlacklistMiddleware 攔截 → 401 token_revoked

```

## 測試流程

用 Swagger 或 Postman 都可以，以下用最直接的步驟測試：

---

### Step 1 — 先登入，取得 token pair

`POST /api/auth/login`

```json
{
  "email": "test@example.com",
  "password": "your_password"
}
```

成功後會拿到：

```json
{
  "id": "...",
  "token": "eyJhbGci...",
  "refreshToken": "abc123xyz...",
  "email": "test@example.com",
  "displayName": "...",
  "role": "User"
}
```

把 `token` 和 `refreshToken` 都複製起來。

---

### Step 2 — 呼叫 Refresh 端點

`POST /api/auth/refresh`

```json
{
  "refreshToken": "abc123xyz..."
}
```

成功的話應該回傳**新的** token pair：

```json
{
  "id": "...",
  "token": "eyJhbGci...（新的）",
  "refreshToken": "def456uvw...（新的）",
  "email": "test@example.com",
  "displayName": "...",
  "role": "User"
}
```

---

### Step 3 — 驗證舊 RefreshToken 已失效（Token Rotation）

再用**Step 1 的舊 refreshToken** 呼叫一次：

`POST /api/auth/refresh`

```json
{
  "refreshToken": "abc123xyz...（舊的）"
}
```

應該要回傳 `401`：

```json
{
  "error": "refresh_token_revoked"
}
```

---

### Step 4 — 確認 DB 欄位有更新

在 SSMS 執行：

```sql
SELECT Id, Email, RefreshToken, RefreshTokenExpiry
FROM Users
WHERE Email = 'test@example.com';
```

確認：
- `RefreshToken` 是 Step 2 回傳的**新值**
- `RefreshTokenExpiry` 是現在時間 + 180 天左右

---

### 測試錯誤情境

**送空值：**

```json
{
  "refreshToken": ""
}
```
→ 預期 `400 Bad Request`

**送亂碼：**

```json
{
  "refreshToken": "this_is_fake"
}
```
→ 預期 `401`，`error: "refresh_token_revoked"`