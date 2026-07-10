// CustomWebApplicationFactory.cs
using DigitalProject.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace DigitalProject.Tests.Helpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private const string DbName = "TestDb";  // ← 固定名稱

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 1. 移除真實 DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                         typeof(DbContextOptions<DigitalVaultStoreDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // 2. 換成記憶體資料庫（固定名稱）
                services.AddDbContext<DigitalVaultStoreDbContext>(options =>
                    options.UseInMemoryDatabase(DbName));  // ← 固定名稱

                // 3. 在這裡直接植入資料！
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider
                    .GetRequiredService<DigitalVaultStoreDbContext>();
                db.Database.EnsureCreated();
                SeedData.Initialize(db);
            });
        }

        public HttpClient CreateClientWithSeedData()
        {
            return CreateClient();  // ← 資料已在 ConfigureWebHost 植入
        }
        public HttpClient CreateClientWithCookies()
        {
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true  // ← 自動保存和帶入 Cookie！
            });
        }

        public HttpClient CreateAdminClient()
        {
            var client = CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            });

            // 1. 先用一般帳號註冊
            client.PostAsJsonAsync("/api/auth/register", new
            {
                email = "admintest@test.com",
                password = "Admin1234",
                displayName = "管理員測試"
            }).Wait();

            // 2. 用 DbContext 直接升級為 admin
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider
                .GetRequiredService<DigitalVaultStoreDbContext>();

            var user = db.Users.FirstOrDefault(u => u.Email == "admintest@test.com");
            var adminRole = db.Roles.FirstOrDefault(r => r.Code == "admin");

            if (user != null && adminRole != null)
            {
                // 移除預設的 user 角色
                var oldRoles = db.UserRoles.Where(ur => ur.UserId == user.Id);
                db.UserRoles.RemoveRange(oldRoles);

                // 加上 admin 角色
                db.UserRoles.Add(new Models.UserRole
                {
                    UserId = user.Id,
                    RoleId = adminRole.Id,
                    AssignedAt = DateTime.UtcNow
                });
                db.SaveChanges();
            }

            // 3. 登入
            client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "admintest@test.com",
                password = "Admin1234"
            }).Wait();

            return client;
        }
    }
}