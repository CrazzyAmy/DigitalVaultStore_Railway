// Response/ReviewResponse.cs
namespace DigitalProject.Response
{
    public class ReviewResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserDisplayName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
