using DigitalProject.Domain;

namespace DigitalProject.Models
{
    public partial class Payment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? VoidByUserId { get; set; }
        public PaymentProvider Provider { get; set; }
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; set; }
        public bool IsVoid { get; set; } = false;
        public string? VoidReason { get; set; }
        public DateTime? VoidAt { get; set; }
        public string? PaymentCode { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public Order Order { get; set; } = null!;
        public User? VoidByUser { get; set; }


    }
}