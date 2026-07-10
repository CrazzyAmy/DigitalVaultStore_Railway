using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Category
{
    public interface ICategoryRepository
    {
        // 前台
        Task<IEnumerable<CategoryResponse>> GetAllAsync();

        // 後台
        Task<IEnumerable<CategoryResponse>> GetAllAdminAsync();
        Task<Models.Category?> GetByIdAsync(Guid id);
        Task<Models.Category> CreateAsync(CreateCategoryRequest request);
        Task UpdateAsync(Guid id, UpdateCategoryRequest request);
        Task DeleteAsync(Guid id);
    }
}
