// Controllers/AdminPaymentController.cs
using DigitalProject.Interface.Payment;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/admin/payment")]
    [Authorize(Policy = "CanManagePayment")]
    public class AdminPaymentController : BaseController
    {
        private readonly IPaymentServie _paymentService;

        public AdminPaymentController(IPaymentServie paymentService)
        {
            _paymentService = paymentService;
        }

        // GET /api/admin/payment
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var payments = await _paymentService.GetAllAsync(request);
            return Ok(payments);
        }

        // PUT /api/admin/payment/{id}/void
        [HttpPut("{id}/void")]
        [Authorize(Policy = "CanManagePayment")]
        public async Task<IActionResult> Void(
            Guid id, [FromBody] VoidPaymentRequest request)
        {
            var adminUserId = GetUserId()!.Value;
            var result = await _paymentService.VoidAsync(adminUserId, id, request.Reason);
            return Ok(result);
        }
    }
}