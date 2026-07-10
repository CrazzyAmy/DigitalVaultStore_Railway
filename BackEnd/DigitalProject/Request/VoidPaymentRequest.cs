using System.ComponentModel.DataAnnotations;

namespace DigitalProject.Request
{
    public class VoidPaymentRequest
    {
        [Required]
        public string Reason { get; set; } = null!;
    }
}
