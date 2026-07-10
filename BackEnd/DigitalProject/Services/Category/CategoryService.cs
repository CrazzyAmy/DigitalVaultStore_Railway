using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.Category;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICacheService _cacheService;
        public CategoryService(ICategoryRepository categoryRepository, ICacheService cacheService)
        {
            _categoryRepository = categoryRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            var cacheKey = "categories:all";

            var cached = await _cacheService.GetAsync<IEnumerable<CategoryResponse>>(cacheKey);
            if (cached != null)
                return cached;

            var result = await _categoryRepository.GetAllAsync();
            // 分類快取 10 分鐘（比商品久，因為較少變動）
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

            return result;
        }
        public async Task<IEnumerable<CategoryResponse>> GetAllAdminAsync() =>
        await _categoryRepository.GetAllAdminAsync();

        public async Task<CategoryResponse?> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new AppException("分類不存在", 404);

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                SortOrder = category.SortOrder,
                IsVisible = category.IsVisible
            };
        }
        // 新增 / 修改 / 刪除分類時清除快取
        public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
        {
            var category = await _categoryRepository.CreateAsync(request);
            // 清除分類快取
            await _cacheService.RemoveAsync("categories:all");
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                SortOrder = category.SortOrder,
                IsVisible = category.IsVisible
            };
        }

        public async Task UpdateAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new AppException("分類不存在", 404);

            await _categoryRepository.UpdateAsync(id, request);
            // 清除分類快取
            await _cacheService.RemoveAsync("categories:all");
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new AppException("分類不存在", 404);

            await _categoryRepository.DeleteAsync(id);
            // 清除快取
            await _cacheService.RemoveAsync("categories:all");  
        }
    }
}
