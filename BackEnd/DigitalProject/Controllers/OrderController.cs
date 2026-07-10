using DigitalProject.Interface.Orders;
using DigitalProject.Models;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST api/order
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            // Auth 完成後換成從 JWT 取得
            var userId = GetUserId()!.Value;
            //var userId = Guid.Parse("1ED3C1A5-5D92-4D42-B29B-60957E3400A2");

            var order = await _orderService.CreateOrderAsync(userId, request);
            return Ok(order);
        }

        // GET api/order/my
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            //var userId = Guid.Parse("1ED3C1A5-5D92-4D42-B29B-60957E3400A2");
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        // GET api/order/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            if (order.UserId != userId)  //新增：驗證本人
                return Forbid();
            return Ok(order);
        }

        // PUT api/order/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _orderService.CancelOrderAsync(userId, id);
            return Ok(new { message = "訂單已取消" });
        }

        // GET /api/order/{orderId}/download
        [HttpGet("{orderId}/download")]
        public async Task<IActionResult> GetDownload(Guid orderId)
        {
            var userId = GetUserId()!.Value;
            var result = await _orderService.GetDownloadAsync(userId, orderId);
            return Ok(result);
        }

        // GET api/order
        // 分頁查詢（保留為根 GET，避免與其它無路徑的 GET 衝突）
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var userId = GetUserId()!.Value;
            var result = await _orderService.GetUserOrdersAsync(userId, request);
            return Ok(result);
        }
    }
}


