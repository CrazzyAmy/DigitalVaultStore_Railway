// Repositories/ReviewRepository.cs
using DigitalProject.Data;
using DigitalProject.Interface.Reviews;
using DigitalProject.Models;
using Microsoft.EntityFrameworkCore;
using DigitalProject.Domain;
using DigitalProject.Response;
using DigitalProject.Request;

namespace DigitalProject.Repositories.Reviews
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DigitalVaultStoreDbContext _context;

        public ReviewRepository(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<Review>> GetByProductIdAsync(Guid productId) =>
            await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<Review>> GetByUserIdAsync(Guid userId) =>
            await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Review?> GetByIdAsync(Guid id) =>
            await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

        // 確認是否已評論過（用 userId + productId）
        public async Task<bool> HasReviewedAsync(Guid userId, Guid productId) =>
            await _context.Reviews
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.ProductId == productId);

        // 確認是否已購買
        public async Task<bool> HasPurchasedAsync(Guid userId, Guid productId) =>
            await _context.OrderItems
                   .AnyAsync(oi =>
                    oi.ProductId == productId &&
                    oi.Order.UserId == userId &&
                    (oi.Order.Status == OrderStatus.Paid ||
                     oi.Order.Status == OrderStatus.Completed));


        // 防止同一筆訂單對同一商品重複評論
        public async Task<bool> ExistsAsync(Guid userId, Guid productId, Guid orderId) =>
            await _context.Reviews
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.ProductId == productId &&
                    r.OrderId == orderId);

        public async Task CreateAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;
            _context.Reviews.Remove(review);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<List<Review>> GetAllAsync() =>
        await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        public async Task<PagedResponse<Review>> GetAllPagedAsync(PagedRequest request)
        {
            var queryable = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .AsQueryable();

            var total = await queryable.CountAsync();

            var data = await queryable
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<Review>
            {
                Data = data,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}