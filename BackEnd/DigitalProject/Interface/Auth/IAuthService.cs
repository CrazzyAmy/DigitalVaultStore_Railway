using DigitalProject.Response;
using DigitalProject.Request;

namespace DigitalProject.Interface.Auth
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> GoogleLoginAsync(string email, string displayName, string providerKey, string? avatarUrl);
        Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
        Task LogoutAsync(Guid userId);
    }
}
