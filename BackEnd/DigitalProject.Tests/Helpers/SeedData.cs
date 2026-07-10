// DigitalProject.Tests/Helpers/SeedData.cs
using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Models;

namespace DigitalProject.Tests.Helpers
{
    public static class SeedData
    {
        public static void Initialize(DigitalVaultStoreDbContext db)
        {
            if (db.Roles.Any()) return;  // 避免重複植入

            // ── 角色 ──────────────────────────────────────────
            var userRole = new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "一般使用者",
                Code = "user",
                CreatedAt = DateTime.UtcNow
            };
            var adminRole = new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "系統管理員",
                Code = "admin",
                CreatedAt = DateTime.UtcNow
            };
            var managerRole = new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "商品管理員",
                Code = "manager",
                CreatedAt = DateTime.UtcNow
            };
            var supportRole = new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                Name = "客服人員",
                Code = "support",
                CreatedAt = DateTime.UtcNow
            };
            db.Roles.AddRange(userRole, adminRole, managerRole, supportRole);

            // ── 分類 ──────────────────────────────────────────
            var category = new Category
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000030"),
                Name = "音樂",
                Slug = "music",
                Description = "音樂相關商品",
                IsVisible = true,
                SortOrder = 1
            };
            db.Categories.Add(category);

            // ── 商品 ──────────────────────────────────────────
            var product = new Models.Product
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000040"),
                Name = "測試商品",
                Description = "這是一個測試商品",
                Price = 100,
                CategoryId = category.Id,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                DownloadUrl = "https://example.com/download",
            };
            var unpublishedProduct = new Models.Product
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000041"),
                Name = "未上架商品",
                Description = "這是一個未上架商品",
                Price = 200,
                CategoryId = category.Id,
                IsPublished = false,  // ← 未上架
                CreatedAt = DateTime.UtcNow,
            };
            db.Products.AddRange(product, unpublishedProduct);

            db.SaveChanges();
        }
    }
}