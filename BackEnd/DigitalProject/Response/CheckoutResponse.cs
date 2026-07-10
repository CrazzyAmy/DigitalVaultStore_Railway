namespace DigitalProject.Response
{
    public class CheckoutResponse
    {
        public string OrderNo { get; set; } = null!;
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Provider { get; set; } = null!;
        public string Status { get; set; } = null!;

        // 超商專用
        public string? PaymentCode { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
