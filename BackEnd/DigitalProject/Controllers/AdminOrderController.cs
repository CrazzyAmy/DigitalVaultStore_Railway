// Controllers/AdminOrderController.cs
using DigitalProject.Interface.Orders;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/admin/order")]
    [Authorize(Policy = "CanViewOrders")]
    public class AdminOrderController : BaseController
    {
        private readonly IOrderService _orderService;

        public AdminOrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET /api/admin/order
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _orderService.GetAllAdminAsync(request);
            return Ok(result);
        }

        // GET /api/admin/order/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetByIdAdminAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // PUT /api/admin/order/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id, [FromBody] UpdateOrderRequest request)
        {
            await _orderService.UpdateOrderStatusAsync(id, request);
            return Ok(new { message = "訂單狀態已更新" });
        }
    }
}