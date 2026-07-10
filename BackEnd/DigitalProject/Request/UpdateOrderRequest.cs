using System.ComponentModel.DataAnnotations;
using DigitalProject.Domain;

namespace DigitalProject.Request
{
    public class UpdateOrderRequest
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}