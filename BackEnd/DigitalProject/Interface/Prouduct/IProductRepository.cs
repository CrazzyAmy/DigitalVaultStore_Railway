using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Prouduct
{
    public interface IProductRepository
    {
        //前台
        //Task<IEnumerable<ProductResponse>> GetAllAsync(bool onlyPublish = true);
        //Task<IEnumerable<ProductResponse>> GetByCategoryAsync(Guid categoryId);
        Task<PagedResponse<ProductResponse>> GetAllAsync(ProductQueryRequest query);
        Task<ProductResponse?> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductResponse>> GetByIdsAsync(List<Guid> ids);

        // 後台
        Task<ProductResponse?> GetByIdAdminAsync(Guid id);    //不過濾 IsPublished
        Task<Product> CreateAsync(CreateProductRequest request);
        Task UpdateAsync(Guid id, UpdateProductRequest request);  // 編輯商品資訊
        Task PublishAsync(Guid id);                               // ← 新增：上架
        Task UnpublishAsync(Guid id);                             // 下架
        Task<IEnumerable<ProductResponse>> GetAllAdminAsync();
    
    }
}
