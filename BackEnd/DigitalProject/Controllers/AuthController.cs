// Controllers/AuthController.cs
using DigitalProject.Exceptions;
using DigitalProject.Interface.Auth;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Tree;
using System.Security.Claims;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ITokenBlacklistService _blacklistService;
        //private void SetRefreshTokenCookie(string refreshToken)
        //{
        //    var cookieOptions = new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true,
        //        SameSite = SameSiteMode.Strict,
        //        Expires = DateTime.UtcNow.AddDays(7), // 與 Refresh Token 的有效期一致
        //        Path = "/api/auth/refresh",
        //        IsEssential = true
        //    };
        //    Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        //}

        public AuthController(IAuthService authService, ITokenBlacklistService blacklistService)
        {
            _authService = authService;
            _blacklistService = blacklistService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            SetTokenCookies(result.Token, result.RefreshToken);

            // Body 只回傳使用者資訊，不含 Token
            return Ok(new
            {
                result.Id,
                result.Email,
                result.DisplayName,
                result.Role,
                result.AvatarUrl
            });
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { error = "invalid_token" });

            try
            {
                var result = await _authService.RefreshAsync(request);

                // ← 更新 Cookie
                SetTokenCookies(result.Token, result.RefreshToken);

                return Ok(new
                {
                    result.Id,
                    result.Email,
                    result.DisplayName,
                    result.Role
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // 1. 從 Cookie 取得 Token 加入黑名單
            var token = Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(token))
                _blacklistService.Blacklist(token, DateTime.UtcNow.AddDays(2));

            // 2. 清除 DB 的 RefreshToken
            var userId = GetUserId()!.Value;
            await _authService.LogoutAsync(userId);

            // 3. 清除 Cookie
            DeleteTokenCookies();

            return Ok(new { message = "登出成功" });
        }


        // GET /api/auth/google
        // 導向 Google 授權頁面
        [HttpGet("google")]
        [AllowAnonymous]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Auth",
                    new { returnUrl }, Request.Scheme)
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // GET /api/auth/google/callback
        // Google 授權後回呼
        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = "/")
        {
            // 1. 取得 Google 回傳的使用者資訊
            var result = await HttpContext.AuthenticateAsync(
        GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                throw new AppException("Google 登入失敗", 401);

            var claims = result.Principal!.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var providerKey = claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier)?.Value;
            var avatarUrl = claims.FirstOrDefault(c =>
                c.Type == "urn:google:picture")?.Value;

            if (email == null || providerKey == null)
                throw new AppException("無法取得 Google 使用者資訊", 401);

            var authResult = await _authService.GoogleLoginAsync(
                email, name ?? email, providerKey, avatarUrl);

            // ← 存入 HttpOnly Cookie
            SetTokenCookies(authResult.Token, authResult.RefreshToken);

            // 導向前端（不帶 Token，改用 Cookie）
            var frontendUrl = $"http://localhost:5173/auth/callback" +
                       $"?id={authResult.Id}" +
                       $"&displayName={Uri.EscapeDataString(authResult.DisplayName)}" +
                       $"&email={Uri.EscapeDataString(authResult.Email)}" +
                       $"&role={authResult.Role}" +
                       $"&avatarUrl={Uri.EscapeDataString(authResult.AvatarUrl ?? "")}";

            return Redirect(frontendUrl);
        }

        // ── Cookie Helper ──────────────────────────────────────────
        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            var isDev = HttpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment();
            // Testing 環境也視為非正式環境
            var isTesting = HttpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .EnvironmentName == "Testing";

            var secure = !isDev && !isTesting;  // ← Testing 環境不用 Secure

            // 因為現在同源了，用 Lax 就好
            Response.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(2)
            });

            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(180)
            });
        }

        private void DeleteTokenCookies()
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
        }
    }
}
