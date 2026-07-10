using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Payment
{
    public interface IPaymentServie
    {
        Task<PaymentResponse> PayAsync(Guid userId, PaymentRequest request);
        Task<List<PaymentResponse>> GetByOrderIdAsync(Guid orderId, Guid userId);
        Task<PaymentResponse> ConfirmCVSPaymentAsync(Guid paymentId, Guid userId);
        Task<PaymentResponse> VoidAsync(Guid adminUserId, Guid paymentId, string reason);
        Task<PagedResponse<PaymentResponse>> GetAllAsync(PagedRequest request);
        Task<CheckoutResponse> CheckoutAsync(Guid userId, CheckoutRequest request);
    }
}
