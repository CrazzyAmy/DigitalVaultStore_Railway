// Controllers/AdminProductController.cs
using DigitalProject.Interface.Prouduct;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/admin/product")]
    [Authorize(Policy = "CanManageProduct")]
    public class AdminProductController : BaseController
    {
        private readonly IProductService _productService;

        public AdminProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET /api/admin/product
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAdminAsync();
            return Ok(products);
        }

        // GET /api/admin/product/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetByIdAdminAsync(id);
            return Ok(product);
        }

        // POST /api/admin/product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateAsync(request);
            return Ok(product);
        }

        // PUT /api/admin/product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            await _productService.UpdateAsync(id, request);
            return Ok(new { message = "商品已更新" });
        }

        // PUT /api/admin/product/{id}/publish ← 新增
        [HttpPut("{id}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            await _productService.PublishAsync(id);
            return Ok(new { message = "商品已上架" });
        }

        // DELETE /api/admin/product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Unpublish(Guid id)
        {
            await _productService.UnpublishAsync(id);
            return Ok(new { message = "商品已下架" });
        }
    }
}