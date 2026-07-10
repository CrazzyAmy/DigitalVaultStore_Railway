using DigitalProject.Domain;
namespace DigitalProject.Models
{
    public partial class Review
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }  // ✅ 改成 CreatedAt

        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}