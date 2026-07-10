namespace DigitalProject.Response
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
    }
}
