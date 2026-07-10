namespace DigitalProject.Response
{
    public class DownloadResponse
    {
        public string OrderNo { get; set; } = null!;
        public List<DownloadItemResponse> Downloads { get; set; } = new();
    }
    public class DownloadItemResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string DownloadUrl { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
