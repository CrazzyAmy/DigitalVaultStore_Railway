# 預防xss攻擊

預防 XSS 主要是**前端的責任**，後端能做的是輔助。

---

## 責任分工

```
XSS 攻擊鏈：
惡意 JS 注入頁面 → 讀取 localStorage → 竊取 token → 打 API

前端負責：阻斷第一步（不讓惡意 JS 進來）
後端負責：就算 token 被偷，降低損害
```

---

## 後端已經做到的

你們目前的實作已經涵蓋了後端能做的最重要部分：

| 機制 | 效果 |
|------|------|
| Login Token 只有 2 天 | 就算被偷，最多用 2 天 |
| Token Rotation | Refresh 後舊 token 立即失效 |
| Blacklist | Logout 後舊 token 立即失效 |
| Logout 要實作黑名單 | 使用者發現異常可以主動登出讓 token 廢掉 |

---

## 後端還可以補強的

### 1. Logout 時把 Login Token 也加入 Blacklist

這個目前可能還沒做，是最重要的補強：

```csharp
// Request/LogoutRequest.cs
public class LogoutRequest
{
    public string LoginToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
```

```csharp
// AuthService.cs
public async Task LogoutAsync(LogoutRequest request)
{
    // 把 Login Token 加入 blacklist
    if (!string.IsNullOrWhiteSpace(request.LoginToken))
        _blacklistService.Blacklist(request.LoginToken, DateTime.UtcNow.AddDays(2));

    // 清除 DB 裡的 Refresh Token
    var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
    if (user != null)
    {
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepository.UpdateRefreshTokenAsync(user);
    }
}
```

```csharp
// AuthController.cs
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
{
    await _authService.LogoutAsync(request);
    return Ok(new { message = "登出成功" });
}
```

---

### 2. 前端需要做的（告知隊友）

後端擋不住 XSS，這些要跟前端同學說：

```
✅ 不用 dangerouslySetInnerHTML
✅ 不引入來路不明的 npm 套件
✅ 所有使用者輸入都要 escape 後再顯示
✅ 使用 DOMPurify 處理任何 HTML 內容
```

---

### 3. 現階段(localstorage)vs. 未來(httpOnly cookie)
目前課程專案是把 token 存在 localStorage，這是為了簡化前端實作，但也讓 token 更容易被 XSS 偷走。未來如果改成 httpOnly cookie，雖然能防止 XSS 直接偷 token，但還是要做好前端的防護，因為攻擊者可能會竊取 cookie 或利用 CSRF 等其他手段。

HttpOnlyCookieAuthFlow.svg架構圖展示了四個完整的 Flow，以下說明與你們目前 `localStorage` 方案的**關鍵差異**：

**Refresh Token 的位置不同：**

| | 目前方案 | HttpOnly Cookie 方案 |
|---|---|---|
| Refresh Token 存放 | `localStorage`（JS 可讀） | HttpOnly Cookie（JS 無法讀） |
| 傳送方式 | 手動放入 Request Body | 瀏覽器自動帶上 |
| XSS 可竊取 | ✅ 可以 | ❌ 不行 |
| Request Body 改動 | 需要帶 `refreshToken` | 不需要，Body 為空 |

**Login Token (JWT) 不變：** 兩種方案都一樣，JWT 存在前端（memory 或 localStorage），每次請求手動放進 `Authorization: Bearer` header。

**後端需要新增的設定（改成 Cookie 方案時）：**

```csharp
// AuthController.cs - 登入時 Set-Cookie
Response.Cookies.Append("refresh_token", authResponse.RefreshToken, new CookieOptions
{
    HttpOnly = true,       // JS 無法讀取
    Secure = true,         // 僅 HTTPS
    SameSite = SameSiteMode.Strict, // 防 CSRF
    Expires = DateTime.UtcNow.AddDays(180)
});

// 回傳 body 只給 JWT，不給 RefreshToken
return Ok(new { token = authResponse.Token, ... });
```

```csharp
// AuthController.cs - Refresh 時從 Cookie 讀取
var refreshToken = Request.Cookies["refresh_token"];
```

課程專案用 `localStorage` 完全沒問題，這個架構圖是供你理解「如果要改成 Cookie 方案，整個流程長什麼樣子」。


## 總結

課程專案的層級，你們後端的防護已經足夠。最值得現在補上的只有一個：

> **Logout 時把 Login Token 加入 Blacklist**

這樣使用者一旦察覺帳號異常，主動登出就能立即讓所有 token 失效，是最實用的防線。

