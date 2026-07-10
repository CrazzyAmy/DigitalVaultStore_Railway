// Interface/IReviewRepository.cs
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
using System.Threading.Tasks;

namespace DigitalProject.Interface.Reviews
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetByProductIdAsync(Guid productId);
        Task<List<Review>> GetByUserIdAsync(Guid userId);
        Task<Review?> GetByIdAsync(Guid id);
        Task<bool> HasReviewedAsync(Guid userId, Guid productId);
        Task<bool> HasPurchasedAsync(Guid userId, Guid productId);
        Task<bool> ExistsAsync(Guid userId, Guid productId, Guid orderId);
        Task CreateAsync(Review review);
        Task<bool> UpdateAsync(Review review);
        Task<bool> DeleteAsync(Guid id);
        Task<List<Review>> GetAllAsync();
        Task<PagedResponse<Review>> GetAllPagedAsync(PagedRequest request);
    }
}
