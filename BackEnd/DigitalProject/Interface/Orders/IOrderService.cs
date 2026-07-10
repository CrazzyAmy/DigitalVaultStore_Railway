using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Orders
{
    public interface IOrderService
    {
        // 前台
        Task<OrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request);
        Task<List<OrderResponse>> GetUserOrdersAsync(Guid userId);
        Task<OrderResponse?> GetOrderByIdAsync(Guid id);
        Task<bool> CancelOrderAsync(Guid userId, Guid orderId);
        Task<DownloadResponse> GetDownloadAsync(Guid userId, Guid orderId);
        Task<PagedResponse<OrderResponse>> GetUserOrdersAsync(Guid userId, PagedRequest request);
        Task<PagedResponse<OrderResponse>> GetAllAdminAsync(PagedRequest request);

        // 後台
        Task<IEnumerable<OrderResponse>> GetAllAdminAsync();
        Task<OrderResponse?> GetByIdAdminAsync(Guid id);        
        Task UpdateOrderStatusAsync(Guid id, UpdateOrderRequest request);  
    }
}