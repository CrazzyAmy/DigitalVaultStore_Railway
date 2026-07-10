// Request/UpdateReviewRequest.cs
namespace DigitalProject.Request
{
    public class UpdateReviewRequest
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}