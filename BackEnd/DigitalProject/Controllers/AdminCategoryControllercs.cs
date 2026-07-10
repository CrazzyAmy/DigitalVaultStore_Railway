using DigitalProject.Interface;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/admin/category")]
    [Authorize(Policy = "CanManageProduct")]
    public class AdminCategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET /api/admin/category
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAdminAsync();
            return Ok(categories);
        }

        // GET /api/admin/category/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(category);
        }

        // POST /api/admin/category
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var category = await _categoryService.CreateAsync(request);
            return Ok(category);
        }

        // PUT /api/admin/category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            await _categoryService.UpdateAsync(id, request);
            return Ok(new { message = "分類已更新" });
        }

        // DELETE /api/admin/category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _categoryService.DeleteAsync(id);
            return Ok(new { message = "分類已刪除" });
        }
    }
}
