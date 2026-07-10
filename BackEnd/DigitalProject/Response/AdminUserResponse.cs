// Response/AdminUserResponse.cs
namespace DigitalProject.Response
{
    public class AdminUserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public string Provider { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();  // Role Code 清單
    }
}