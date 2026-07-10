// Controllers/ReviewController.cs
using DigitalProject.Exceptions;
using DigitalProject.Interface.Reviews;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET /api/review/product/{productId}
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProduct(Guid productId)
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId);
            return Ok(reviews);
        }

        // GET /api/review/product/{productId}/stats
        [HttpGet("product/{productId}/stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStats(Guid productId)
        {
            var stats = await _reviewService.GetStatsAsync(productId);
            return Ok(stats);
        }

        // GET /api/review/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = GetUserId()!.Value;
            var reviews = await _reviewService.GetByUserIdAsync(userId);
            return Ok(reviews);
        }

        // GET /api/review/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null)
                throw new AppException("評論不存在", 404);
            return Ok(review);
        }

        // POST /api/review
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
        {
            var userId = GetUserId()!.Value;
            var review = await _reviewService.CreateAsync(userId, request);
            return Ok(review);
        }

        // PUT /api/review/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewRequest request)
        {
            var userId = GetUserId()!.Value;
            await _reviewService.UpdateAsync(userId, id, request);
            return Ok(new { message = "評論已更新" });
        }

        // DELETE /api/review/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId()!.Value;
            await _reviewService.DeleteAsync(userId, id);
            return Ok(new { message = "評論已刪除" });
        }

    }
}