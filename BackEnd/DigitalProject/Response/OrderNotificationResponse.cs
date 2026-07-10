namespace DigitalProject.Response
{
    public class OrderNotificationResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNo { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Provider { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
