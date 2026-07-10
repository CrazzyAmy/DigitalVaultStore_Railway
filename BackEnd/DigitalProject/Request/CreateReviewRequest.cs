// Request/CreateReviewRequest.cs
using System.ComponentModel.DataAnnotations;

namespace DigitalProject.Request
{
    public class CreateReviewRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Range(1, 5, ErrorMessage = "評分必須在 1 到 5 之間")]
        public int Rating { get; set; }

        [Required]
        [MaxLength(1000, ErrorMessage = "評論最多 1000 字")]
        public string? Comment { get; set; }
    }
}