// Services/Product/ProductService.cs
using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Services.Prouduct
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICacheService _cacheService;

        public ProductService(IProductRepository productRepository,
            ICacheService cacheService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
        }

        // ── 前台 ──────────────────────────────────────────────

        public async Task<PagedResponse<ProductResponse>> GetAllAsync(ProductQueryRequest query)
        {
            // 產生快取 Key（根據查詢條件）
            var cacheKey = $"products:" +
           $"page={query.Page}:" +
           $"size={query.PageSize}:" +
           $"keyword={query.Keyword}:" +
           $"cat={query.CategoryId}:" +
           $"min={query.MinPrice}:" +
           $"max={query.MaxPrice}:" +
           $"sort={query.SortBy}:{query.SortOrder}";

            // 先查快取
            var cached = await _cacheService
                .GetAsync<PagedResponse<ProductResponse>>(cacheKey);
            if (cached != null)
                return cached;  

            // 快取沒有 → 查 DB
            var result = await _productRepository.GetAllAsync(query);

            // 存入快取（5分鐘）
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;


        }

        public async Task<ProductResponse?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"product:{id}";

            var cached = await _cacheService.GetAsync<ProductResponse>(cacheKey);
            if (cached != null)
                return cached;

            var result = await _productRepository.GetByIdAsync(id);
            if (result != null)
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }


        // ── 後台 ──────────────────────────────────────────────

        public async Task<IEnumerable<ProductResponse>> GetAllAdminAsync()
        {
            // 後台查所有商品（含未發布）
            var query = new ProductQueryRequest();  // 空查詢條件
            var allProducts = await _productRepository.GetAllAsync(query);

            // 但 GetAllAsync 固定過濾 IsPublished
            // 所以後台要用 GetByIdAdminAsync 的概念
            // 直接用 Repository 的後台方法
            return await _productRepository.GetAllAdminAsync();
        }

        public async Task<ProductResponse?> GetByIdAdminAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);
            return product;
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var product = await _productRepository.CreateAsync(request);
            // 清除商品列表快取
            await _cacheService.RemoveByPrefixAsync("products:");
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ThumbnailUrl = product.ThumbnailUrl,
                DownloadUrl = product.DownloadUrl,
                IsPublished = product.IsPublished,
                CreatedAt = product.CreatedAt,
                CategoryId = product.CategoryId,
            };
        }

        public async Task UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);

            await _productRepository.UpdateAsync(id, request);

            // 清除該商品快取 + 列表快取
            await _cacheService.RemoveAsync($"product:{id}");
            await _cacheService.RemoveByPrefixAsync("products:");
        }

        public async Task PublishAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);
            if (product.IsPublished)
                throw new AppException("商品已上架");

            await _productRepository.PublishAsync(id);
            await _cacheService.RemoveAsync($"product:{id}");
            await _cacheService.RemoveByPrefixAsync("products:");
        }

        public async Task UnpublishAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);
            if (!product.IsPublished)
                throw new AppException("商品已下架");

            await _productRepository.UnpublishAsync(id);
            await _cacheService.RemoveAsync($"product:{id}");
            await _cacheService.RemoveByPrefixAsync("products:");
        }

    }
}