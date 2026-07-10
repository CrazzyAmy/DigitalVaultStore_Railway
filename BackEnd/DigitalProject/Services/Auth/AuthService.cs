using DigitalProject.Domain;
using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.Auth;
using DigitalProject.Interface.Role;
using DigitalProject.Interface.User;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
using DigitalProject.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenBlacklistService _blacklistService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtHelper _jwtHelper;
        private readonly IRoleRepository _roleRepository;
        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtHelper jwtHelper, ITokenBlacklistService blacklistService, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtHelper = jwtHelper;
            _blacklistService = blacklistService;
            _roleRepository = roleRepository;
        }
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // 1. 檢查 Email 是否已存在
            if(await _userRepository.IsEmailExistsAsync(request.Email))
                throw new AppException("此 Email 已被註冊");
            var defaultRole = await _roleRepository.GetByCodeAsync("user");
            if (defaultRole == null)
                throw new AppException("系統角色設定錯誤", 500);
            // 2. 建立 User
            var user = new Models.User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                DisplayName = request.DisplayName,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Provider = AuthProvider.Local,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            await _userRepository.CreateAsync(user);
            await _userRepository.AddRoleAsync(user.Id, defaultRole.Id);
            return new RegisterResponse
            {
                Message = "註冊成功，請使用 Email 登入",
                Email = user.Email,
                DisplayName = user.DisplayName,
            };
        }
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            //1.查找User
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                throw new AppException("Email或密碼錯誤",401);
            // 2. 驗證密碼
            if (!_passwordHasher.Verify(request.Password, user.PasswordHash!))
                throw new AppException("Email 或密碼錯誤", 401);
            // 3. 確認帳號啟用
            if (!user.IsActive)
                throw new AppException("此帳號已被停用", 401);
            // 4. 回傳 JWT
            var authResponse = _jwtHelper.GenerateToken(user);

            // 將 Refresh Token 和過期時間寫回使用者紀錄
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(180);
            await _userRepository.UpdateRefreshTokenAsync(user);

            return authResponse;


        }

        public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
        {
            // Step 1：用 RefreshToken 找 User
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken); // ← 改這行
            if (user == null)
                throw new UnauthorizedAccessException("refresh_token_revoked");

            // Step 2：檢查是否過期
            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new UnauthorizedAccessException("refresh_token_expired");

            // Step 3：把舊的 Login Token 加入 blacklist
            // Login Token 有效期是 2 天，所以 expiry 給 UtcNow + 2 天
            _blacklistService.Blacklist(
                request.OldLoginToken,
                DateTime.UtcNow.AddDays(2)
            );

            // Step 4：產生新的 token pair
            var authResponse = _jwtHelper.GenerateToken(user);

            // Step 5：把新 RefreshToken 存回 DB
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(180);
            await _userRepository.UpdateRefreshTokenAsync(user);

            return authResponse;
        }

         public async Task<AuthResponse> GoogleLoginAsync(
             string email,
             string displayName,
      string providerKey,
      string? avatarUrl)
        {
            // 1. 先用 ProviderKey 查
            var user = await _userRepository.GetByProviderKeyAsync(providerKey);

            if (user == null)
            {
                // 2. 再用 Email 查
                user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    // 查詢預設角色
                    var defaultRole = await _roleRepository.GetByCodeAsync("user");
                    if (defaultRole == null)
                        throw new AppException("系統角色設定錯誤", 500);
                    // 3. 都找不到 → 自動註冊
                    user = new Models.User
                    {
                        Id = Guid.NewGuid(),
                        Email = email,
                        DisplayName = displayName,
                        AvatarUrl = avatarUrl,
                        Provider = AuthProvider.Google,
                        ProviderKey = providerKey,
                        PasswordHash = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _userRepository.CreateAsync(user);
                    await _userRepository.AddRoleAsync(user.Id, defaultRole.Id);
                }
                else
                {
                    // 4. Email 找到 → 更新 ProviderKey
                    user.ProviderKey = providerKey;
                    user.AvatarUrl = avatarUrl;
                    await _userRepository.UpdateAsync(user);
                }
            }

            if (!user.IsActive)
                throw new AppException("此帳號已被停用", 401);

            // 5. 產生 Token
            var authResponse = _jwtHelper.GenerateToken(user);

            // 6. ← 新增：儲存 RefreshToken（跟 LoginAsync 一樣）
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(180);
            await _userRepository.UpdateRefreshTokenAsync(user);

            return authResponse;
        }

        public  async Task LogoutAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepository.UpdateRefreshTokenAsync(user);

        }
    }
}