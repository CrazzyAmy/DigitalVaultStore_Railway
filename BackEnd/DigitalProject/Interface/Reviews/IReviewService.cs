// Interface/IReviewService.cs
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Reviews
{
    public interface IReviewService
    {
        Task<List<ReviewResponse>> GetByProductIdAsync(Guid productId);
        Task<List<ReviewResponse>> GetByUserIdAsync(Guid userId);
        Task<ReviewResponse?> GetByIdAsync(Guid id);
        Task<ReviewStatsResponse> GetStatsAsync(Guid productId);              // ← 新增
        Task<ReviewResponse> CreateAsync(Guid userId, CreateReviewRequest request);    // ← 拿掉 tuple
        Task UpdateAsync(Guid userId, Guid reviewId, UpdateReviewRequest request);     // ← 拿掉 tuple
        Task DeleteAsync(Guid userId, Guid reviewId);                                  // ← 拿掉 tuple
        Task AdminDeleteAsync(Guid reviewId);
        Task<PagedResponse<ReviewResponse>> GetAllAsync(PagedRequest request);
    }
}