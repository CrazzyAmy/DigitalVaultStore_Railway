// Interface/Product/IProductService.cs
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Prouduct
{
    public interface IProductService
    {
        // 前台
        Task<PagedResponse<ProductResponse>> GetAllAsync(ProductQueryRequest query);
        Task<ProductResponse?> GetByIdAsync(Guid id);

        // 後台
        Task<IEnumerable<ProductResponse>> GetAllAdminAsync();
        Task<ProductResponse?> GetByIdAdminAsync(Guid id);
        Task<ProductResponse> CreateAsync(CreateProductRequest request);
        Task UpdateAsync(Guid id, UpdateProductRequest request);
        Task PublishAsync(Guid id);    // ← 新增
        Task UnpublishAsync(Guid id);
    }
}