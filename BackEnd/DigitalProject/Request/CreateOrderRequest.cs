using System.ComponentModel.DataAnnotations;

namespace DigitalProject.Request
{
    public class CreateOrderRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "至少要有一個商品")]
        public List<Guid> ProductIds { get; set; } = new();
    }
}
