using DigitalProject.Domain;

namespace DigitalProject.Response
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNo { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusName => Status switch
        {
            PaymentStatus.Pending => "待付款",
            PaymentStatus.Paid => "已付款",
            PaymentStatus.Failed => "付款失敗",
            PaymentStatus.Refunded => "已退款",
            _ => "未知狀態"
        };
        public string Provider { get; set; } = null!;
        public DateTime?PaidAt { get; set; }
        public bool IsVoid { get; set; }
        public string? VoidReason { get; set; }   
        public DateTime? VoidAt { get; set; }      

        //超商繳費用
        public string? PaymentCode { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // 後台新增
        public string? UserEmail { get; set; }
        public string? UserDisplayName { get; set; }
    }
    
}
