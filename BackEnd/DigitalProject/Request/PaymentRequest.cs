using DigitalProject.Domain;
using System.ComponentModel.DataAnnotations;

namespace DigitalProject.Request
{
    public class PaymentRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public PaymentProvider Provider { get; set; }

        //信用卡支付需要填
        public string? CardNumber { get; set; }
        public string? CardHolder { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Cvv { get; set; }


    }
}
