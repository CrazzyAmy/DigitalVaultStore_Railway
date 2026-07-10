using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface
{
    public interface ICategoryService
    {
        // 前台
        Task<IEnumerable<CategoryResponse>> GetAllAsync();

        // 後台
        Task<IEnumerable<CategoryResponse>> GetAllAdminAsync();
        Task<CategoryResponse?> GetByIdAsync(Guid id);
        Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
        Task UpdateAsync(Guid id, UpdateCategoryRequest request);
        Task DeleteAsync(Guid id);
    }
}
