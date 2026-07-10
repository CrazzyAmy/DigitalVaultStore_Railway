using DigitalProject.Interface.Payment;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IPaymentServie _paymentService;

        public PaymentController(IPaymentServie  paymentService)
        {
            _paymentService = paymentService;
        }

        // POST /api/payment
        // 信用卡或超商付款
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Pay([FromBody] PaymentRequest request)
        {
            var userId = GetUserId()!.Value;
            var result = await _paymentService.PayAsync(userId, request);
            return Ok(result);
        }

        // GET /api/payment/order/{orderId}
        // 取得訂單所有付款紀錄
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var userId = GetUserId()!.Value;
            var payments = await _paymentService.GetByOrderIdAsync(orderId, userId);
            return Ok(payments);
        }

        // PUT /api/payment/{id}/cvs-confirm
        // 模擬超商繳費完成
        [HttpPut("{id}/cvs-confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmCVS(Guid id)
        {
            var userId = GetUserId()!.Value;
            var result = await _paymentService.ConfirmCVSPaymentAsync(id, userId);
            return Ok(result);
        }
        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userId = GetUserId()!.Value;
            var result = await _paymentService.CheckoutAsync(userId, request);
            return Ok(result);
        }


    }
}
