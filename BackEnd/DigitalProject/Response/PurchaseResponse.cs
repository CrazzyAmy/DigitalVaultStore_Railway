namespace DigitalProject.Response
{
    public class PurchaseResponse
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime PurchasedAt { get; set; }
    }
}
