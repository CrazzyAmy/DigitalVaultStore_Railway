using DigitalProject.Domain;
using DigitalProject.Exceptions;
using DigitalProject.Interface.Orders;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
namespace DigitalProject.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;

        }
        public async Task<OrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request)
        {
            var products = (await _productRepository.GetByIdsAsync(request.ProductIds)).ToList();
            if (products.Count == 0)
                throw new AppException("找不到任何有效商品", 404);
            var items = products.Select(p => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = p.Id,
                ProductName = p.Name,
                UnitPrice = p.Price,
                Quantity = 1,
                SubTotal = p.Price
            }).ToList();
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNo = "DV-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                TotalAmount = items.Sum(i => i.SubTotal),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderItems = items,

            };
            await _orderRepository.CreateAsync(order);
            return MapToResponse(order);

        }
        public async Task<List<OrderResponse>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);

            return orders
                .Where(o =>
                    o.Status == OrderStatus.Paid ||
                    o.Status == OrderStatus.Completed ||
                    (o.Status == OrderStatus.Pending &&
                     o.Payments.Any(p =>
                         p.Provider == PaymentProvider.CVS &&
                         p.IsVoid == false &&
                         p.Status == PaymentStatus.Pending))
                )
                .Select(MapToResponse)
                .ToList();
        }
        



        public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : MapToResponse(order);
        }
        //取消訂單
        public async Task<bool> CancelOrderAsync(Guid userId, Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new AppException("訂單不存在", 404);

            if (order.UserId != userId)
                throw new AppException("無權限取消此訂單", 403);

            if (order.Status != OrderStatus.Pending)
                throw new AppException("只有待付款的訂單可以取消");
            await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Cancelled);
            return true;

        }

        public async Task<DownloadResponse> GetDownloadAsync(Guid userId, Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
                throw new AppException("訂單不存在", 404);
            if (order.UserId != userId)
                throw new AppException("無權限", 403);
            if (order.Status == OrderStatus.Pending)
                throw new AppException("請先完成付款", 400);
            if (order.Status == OrderStatus.Cancelled)
                throw new AppException("此訂單已取消", 400);

            // 產生虛擬下載連結（24 小時有效）
            var downloads = order.OrderItems.Select(item => new DownloadItemResponse
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                DownloadUrl = string.IsNullOrEmpty(item.Product?.DownloadUrl) || item.Product.DownloadUrl == "#"
              ? $"https://digitalvault.com/downloads/{item.ProductId}"
              : item.Product.DownloadUrl,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            }).ToList();

            // 付款後第一次下載 → 更新為已完成
            if (order.Status == OrderStatus.Paid)
                await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Completed);

            return new DownloadResponse
            {
                OrderNo = order.OrderNo,
                Downloads = downloads
            };
        }

        // 後台查所有訂單
        public async Task<IEnumerable<OrderResponse>> GetAllAdminAsync()
        {
            var orders = await _orderRepository.GetAllAdminAsync();
            return orders.Select(MapToResponse);
        }

        // 後台查單一訂單
        public async Task<OrderResponse?> GetByIdAdminAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAdminAsync(id);
            return order == null ? null : MapToResponse(order);
        }

        // 後台更新訂單狀態
        public async Task UpdateOrderStatusAsync(Guid id, UpdateOrderRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new AppException("訂單不存在", 404);

            await _orderRepository.UpdateStatusAsync(id, request.Status);
        }


        private OrderResponse MapToResponse(Order o)
         => new()
         {
             Id = o.Id,
             UserId = o.UserId,
             OrderNo = o.OrderNo,
             TotalAmount = o.TotalAmount,
             Status = o.Status,
             CreatedAt = o.CreatedAt,
             UserEmail = o.User?.Email,          
             UserDisplayName = o.User?.DisplayName,
             Items = o.OrderItems.Select(i => new OrderItemResponse
             {
                 Id = i.Id,
                 ProductId = i.ProductId,
                 ProductName = i.ProductName,
                 UnitPrice = i.UnitPrice,
                 Quantity = i.Quantity,
                 SubTotal = i.SubTotal,
             }).ToList(),
         };

        public async Task<PagedResponse<OrderResponse>> GetUserOrdersAsync(Guid userId, PagedRequest request)
        {
            var paged = await _orderRepository.GetUserOrdersPagedAsync(userId, request);
            return new PagedResponse<OrderResponse>
            {
                Data = paged.Data.Select(MapToResponse).ToList(),
                Total = paged.Total,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }

        public async Task<PagedResponse<OrderResponse>> GetAllAdminAsync(PagedRequest request)
        {
            var paged = await _orderRepository.GetAllAdminPagedAsync(request);
            return new PagedResponse<OrderResponse>
            {
                Data = paged.Data.Select(MapToResponse).ToList(),
                Total = paged.Total,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }
    }
}
