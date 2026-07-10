namespace DigitalProject.Models
{
    public partial class Product
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public Category Category { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}