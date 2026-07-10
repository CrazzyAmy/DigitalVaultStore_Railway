using DigitalProject.Domain;

namespace DigitalProject.Response
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }  
        public string OrderNo { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName => Status switch   // 新增中文顯示
        {
            OrderStatus.Pending => "待付款",
            OrderStatus.Paid => "已付款",
            OrderStatus.Completed => "已完成",
            OrderStatus.Cancelled => "已取消",
            _ => "未知"
        };
        public List<OrderItemResponse> Items { get; set; } = new();

        // 後台新增
        public string? UserEmail { get; set; }        
        public string? UserDisplayName { get; set; }
    }
}
