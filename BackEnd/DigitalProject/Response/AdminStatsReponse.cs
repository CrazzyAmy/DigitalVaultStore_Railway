namespace DigitalProject.Response
{
    public class AdminStatsResponse
    {
        public int TotalOrders { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalUsers { get; set; }
        public int PendingOrders { get; set; }
        public List<RecentOrderResponse> RecentOrders { get; set; } = new();
    }

    public class RecentOrderResponse
    {
        public Guid Id { get; set; }
        public string OrderNo { get; set; } = null!;
        public string UserDisplayName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
