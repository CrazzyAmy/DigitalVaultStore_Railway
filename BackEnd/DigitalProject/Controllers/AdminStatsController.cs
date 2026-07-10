using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/admin/stats")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminStatsController : BaseController
    {
        private readonly DigitalVaultStoreDbContext _context;

        public AdminStatsController(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }

        // GET /api/admin/stats
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            //只算有效訂單
            var totalOrders = await _context.Orders
                 .Where(o =>
            o.Status == OrderStatus.Paid ||
            o.Status == OrderStatus.Completed ||
            (o.Status == OrderStatus.Pending &&
             o.Payments.Any(p =>
                 p.Provider == PaymentProvider.CVS &&
                 p.IsVoid == false &&
                 p.Status == PaymentStatus.Pending)))
           .CountAsync();

            // 本月營收（已付款 + 已完成）
            var monthlyRevenue = await _context.Orders
         .Where(o =>
             o.CreatedAt >= firstDayOfMonth &&
             (o.Status == OrderStatus.Paid ||
              o.Status == OrderStatus.Completed))
         .SumAsync(o => o.TotalAmount);

            // 總用戶數
            var totalUsers = await _context.Users.CountAsync();

            // 待處理訂單（Pending）
            var pendingOrders = await _context.Orders
        .Where(o =>
            o.Status == OrderStatus.Pending &&
            o.Payments.Any(p =>
                p.Provider == PaymentProvider.CVS &&
                p.IsVoid == false &&
                p.Status == PaymentStatus.Pending))
        .CountAsync();

            // 最近 5 筆訂單
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderResponse
                {
                    Id = o.Id,
                    OrderNo = o.OrderNo,
                    UserDisplayName = o.User.DisplayName,
                    TotalAmount = o.TotalAmount,
                    StatusName = o.Status == OrderStatus.Pending ? "待付款" :
                                      o.Status == OrderStatus.Paid ? "已付款" :
                                      o.Status == OrderStatus.Completed ? "已完成" :
                                      o.Status == OrderStatus.Cancelled ? "已取消" : "未知",
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return Ok(new AdminStatsResponse
            {
                TotalOrders = totalOrders,
                MonthlyRevenue = monthlyRevenue,
                TotalUsers = totalUsers,
                PendingOrders = pendingOrders,
                RecentOrders = recentOrders
            });
        }
    }
}

    

