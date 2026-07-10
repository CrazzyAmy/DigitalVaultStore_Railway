using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Interface.Orders;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories
{
    public class OrderRepository : IOrderRepository 
    {
        private readonly DigitalVaultStoreDbContext _db;
        public OrderRepository(DigitalVaultStoreDbContext db)
        {
            _db = db;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetByIdAsync(Guid id)=>
        
            await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)  // ← 新增，下載功能需要
                .FirstOrDefaultAsync(o => o.Id == id);


        public async Task<List<Order>> GetByUserIdAsync(Guid userId) =>
           await _db.Orders
         .Include(o => o.OrderItems)
             .ThenInclude(i => i.Product)
         .Include(o => o.Payments)  // ← 新增！過濾殭屍訂單需要
         .Where(o => o.UserId == userId)
         .OrderByDescending(o => o.CreatedAt)
         .ToListAsync();
        //更新訂單狀態
        public async Task UpdateStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return;
            order.Status = status;
            await _db.SaveChangesAsync();
        }

        // 後台查所有訂單
        public async Task<List<Order>> GetAllAdminAsync() =>
            await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

        // 後台查單一訂單
        public async Task<Order?> GetByIdAdminAsync(Guid id) =>
            await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<PagedResponse<Order>> GetUserOrdersPagedAsync(Guid userId, PagedRequest request)
        {
            var queryable = _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .Where(o =>
                o.UserId == userId &&
                (o.Status == OrderStatus.Paid ||
                 o.Status == OrderStatus.Completed ||
                 (o.Status == OrderStatus.Pending &&
                  o.Payments.Any(p =>
                      p.Provider == PaymentProvider.CVS &&
                      p.IsVoid == false &&
                      p.Status == PaymentStatus.Pending))))
            .OrderByDescending(o => o.CreatedAt)
            .AsQueryable();

            var total = await queryable.CountAsync();

            var data = await queryable
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<Order>
            {
                Data = data,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResponse<Order>> GetAllAdminPagedAsync(PagedRequest request)
        {
            var queryable = _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .AsQueryable();

            var total = await queryable.CountAsync();

            var data = await queryable
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<Order>
            {
                Data = data,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
