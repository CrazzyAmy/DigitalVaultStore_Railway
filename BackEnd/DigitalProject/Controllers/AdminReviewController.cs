// Controllers/AdminReviewController.cs
using DigitalProject.Interface.Reviews;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/admin/review")]
    [Authorize(Policy = "CanManageReview")]
    public class AdminReviewController : BaseController
    {
        private readonly IReviewService _reviewService;

        public AdminReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET /api/admin/review
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _reviewService.GetAllAsync(request);
            return Ok(result);
        }

        // DELETE /api/admin/review/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _reviewService.AdminDeleteAsync(id);
            return Ok(new { message = "評論已刪除" });
        }
    }
}