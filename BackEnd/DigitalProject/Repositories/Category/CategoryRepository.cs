using DigitalProject.Data;
using DigitalProject.Interface.Category;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DigitalVaultStoreDbContext _context;
        public CategoryRepository(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }

        // 前台：只顯示可見的
        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c=>c.IsVisible)
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    SortOrder = c.SortOrder
                })
                .ToListAsync();
        }
        // 前台：只顯示可見的
        public async Task<IEnumerable<CategoryResponse>> GetAllAdminAsync() =>
            await _context.Categories
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    SortOrder = c.SortOrder,
                    IsVisible = c.IsVisible,
                    ProductCount = c.Products.Count()
                })
                .ToListAsync();
        public async Task<Category?> GetByIdAsync(Guid id) =>
              await _context.Categories.FindAsync(id);

        public async Task<Category> CreateAsync(CreateCategoryRequest request)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                SortOrder = request.SortOrder,
                IsVisible = request.IsVisible
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return;

            category.Name = request.Name;
            category.Slug = request.Slug;
            category.Description = request.Description;
            category.SortOrder = request.SortOrder;
            category.IsVisible = request.IsVisible;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }



    }
}
