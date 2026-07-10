// Services/ReviewService.cs
using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.Reviews;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<List<ReviewResponse>> GetByProductIdAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);
            return reviews.Select(MapToResponse).ToList();
        }

        public async Task<List<ReviewResponse>> GetByUserIdAsync(Guid userId)
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(userId);
            return reviews.Select(MapToResponse).ToList();
        }

        public async Task<ReviewResponse?> GetByIdAsync(Guid id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review == null ? null : MapToResponse(review);
        }

        public async Task<ReviewStatsResponse> GetStatsAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);

            return new ReviewStatsResponse
            {
                TotalCount = reviews.Count,
                AverageRating = reviews.Count > 0
                    ? Math.Round(reviews.Average(r => r.Rating), 1)
                    : 0,
                RatingDistribution = Enumerable.Range(1, 5)
                    .ToDictionary(
                        star => star,
                        star => reviews.Count(r => r.Rating == star))
            };
        }

        public async Task<ReviewResponse> CreateAsync(Guid userId, CreateReviewRequest request)
        {
            //1.確認已購買（IsRequired(false) 期間暫時跳過）
             var hasPurchased = await _reviewRepository.HasPurchasedAsync(userId, request.ProductId);
            if (!hasPurchased)
                throw new AppException("必須購買商品後才能評論", 403);

            // 2. 防止重複評論
            var exists = await _reviewRepository.ExistsAsync(
                userId, request.ProductId, request.OrderId);
            if (exists)
                throw new AppException("此訂單已對該商品評論過");

            // 3. 建立評論
            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = request.ProductId,
                OrderId = request.OrderId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.CreateAsync(review);

            // 重新查詢取得完整資料（含 User、Product）
            var created = await _reviewRepository.GetByIdAsync(review.Id);
            return MapToResponse(created!);
        }

        public async Task UpdateAsync(Guid userId, Guid reviewId, UpdateReviewRequest request)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review == null)
                throw new AppException("評論不存在", 404);

            if (review.UserId != userId)
                throw new AppException("無權限修改此評論", 403);

            review.Rating = request.Rating;
            review.Comment = request.Comment;

            await _reviewRepository.UpdateAsync(review);
        }

        public async Task DeleteAsync(Guid userId, Guid reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review == null)
                throw new AppException("評論不存在", 404);

            if (review.UserId != userId)
                throw new AppException("無權限刪除此評論", 403);

            await _reviewRepository.DeleteAsync(reviewId);
        }

        public async Task AdminDeleteAsync(Guid reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review == null)
                throw new AppException("評論不存在", 404);

            await _reviewRepository.DeleteAsync(reviewId);
        }

        private static ReviewResponse MapToResponse(Review review) => new()
        {
            Id = review.Id,
            UserId = review.UserId,
            UserDisplayName = review.User?.DisplayName ?? string.Empty,
            ProductId = review.ProductId,
            ProductName = review.Product?.Name ?? string.Empty,
            UserAvatarUrl = review.User?.AvatarUrl,
            OrderId = review.OrderId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        public async Task<PagedResponse<ReviewResponse>> GetAllAsync(PagedRequest request)
        {
            var paged = await _reviewRepository.GetAllPagedAsync(request);
            return new PagedResponse<ReviewResponse>
            {
                Data = paged.Data.Select(MapToResponse).ToList(),
                Total = paged.Total,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }
    }
}