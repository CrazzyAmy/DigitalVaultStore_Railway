// Repositories/Product/ProductRepository.cs
using DigitalProject.Data;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DigitalProject.Repositories.Prouduct
{
    public class ProductRepository : IProductRepository
    {
        private readonly DigitalVaultStoreDbContext _context;

        public ProductRepository(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }

        // ── 前台：搜尋所有商品 ──
        public async Task<PagedResponse<ProductResponse>> GetAllAsync(ProductQueryRequest query)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsPublished)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
                products = products.Where(p =>
                    p.Name.Contains(query.Keyword) ||
                    (p.Description != null && p.Description.Contains(query.Keyword)));

            if (query.CategoryId.HasValue)
                products = products.Where(p => p.CategoryId == query.CategoryId);

            if (query.MinPrice.HasValue)
                products = products.Where(p => p.Price >= query.MinPrice);

            if (query.MaxPrice.HasValue)
                products = products.Where(p => p.Price <= query.MaxPrice);

            products = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
            {
                ("price", "asc") => products.OrderBy(p => p.Price),
                ("price", "desc") => products.OrderByDescending(p => p.Price),
                ("createdat", "asc") => products.OrderBy(p => p.CreatedAt),
                ("createdat", "desc") => products.OrderByDescending(p => p.CreatedAt),
                _ => products.OrderByDescending(p => p.CreatedAt)
            };

            // 先取總筆數
            var total = await products.CountAsync();

            // 再分頁
            var data = await products
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResponse<ProductResponse>
            {
                Data = data.Select(MapToResponse).ToList(),
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }


        // ── 前台：查單一商品 ──
        public async Task<ProductResponse?> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id && p.IsPublished)
                .FirstOrDefaultAsync();

            return product == null ? null : MapToResponse(product);
        }

        // ── 前台：依 ID 清單查商品（結帳用）──
        public async Task<IEnumerable<ProductResponse>> GetByIdsAsync(List<Guid> ids)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => ids.Contains(p.Id) && p.IsPublished)
                .ToListAsync();

            return products.Select(MapToResponse);
        }

        // ── 後台：查單一商品（不過濾 IsPublished）──
        public async Task<ProductResponse?> GetByIdAdminAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            return product == null ? null : MapToResponse(product);
        }

        // ── 後台：新增商品 ──
        public async Task<Product> CreateAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                ThumbnailUrl = request.ThumbnailUrl,
                DownloadUrl = request.DownloadUrl,
                IsPublished = request.IsPublished,
                CreatedAt = DateTime.UtcNow,
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }
        public async Task<IEnumerable<ProductResponse>> GetAllAdminAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();  // 不過濾 IsPublished

            return products.Select(MapToResponse);
        }

        // ── 後台：編輯商品 ──
        public async Task UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;
            product.ThumbnailUrl = request.ThumbnailUrl;
            product.DownloadUrl = request.DownloadUrl;
            product.IsPublished = request.IsPublished;

            await _context.SaveChangesAsync();
        }

        // ── 後台：上架 ──
        public async Task PublishAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;
            product.IsPublished = true;
            await _context.SaveChangesAsync();
        }

        // ── 後台：下架 ──
        public async Task UnpublishAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;
            product.IsPublished = false;
            await _context.SaveChangesAsync();
        }

        // ── 共用 MapToResponse ──
        private static ProductResponse MapToResponse(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ThumbnailUrl = string.IsNullOrEmpty(p.ThumbnailUrl)
                ? $"https://picsum.photos/400/220?random={p.Id}"
                : p.ThumbnailUrl,
            DownloadUrl = p.DownloadUrl,
            IsPublished = p.IsPublished,
            CreatedAt = p.CreatedAt,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name
        };

       
    }
}