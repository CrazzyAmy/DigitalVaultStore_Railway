using DigitalProject.Domain;

namespace DigitalProject.Models
{
    public partial class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string OrderNo { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();


    }
}