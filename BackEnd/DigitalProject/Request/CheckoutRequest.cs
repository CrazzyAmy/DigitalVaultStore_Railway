using DigitalProject.Domain;
using System.ComponentModel.DataAnnotations;

namespace DigitalProject.Request
{
    public class CheckoutRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "至少要有一個商品")]
        public List<Guid> ProductIds { get; set; } = new();

        [Required]
        public PaymentProvider Provider { get; set; }

        // 信用卡專用
        public string? CardNumber { get; set; }
        public string? CardHolder { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Cvv { get; set; }
    }
}
