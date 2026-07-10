using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Response
{
    public class AuthResponse
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
