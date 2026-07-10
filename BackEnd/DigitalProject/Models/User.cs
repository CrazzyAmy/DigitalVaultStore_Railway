using DigitalProject.Domain;
namespace DigitalProject.Models
{
    public partial class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public AuthProvider Provider { get; set; } = AuthProvider.Local;
        public string? ProviderKey { get; set; }
        public string? PasswordHash { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Payment> VoidedPayments { get; set; } = new List<Payment>();
    }
}