using System.Net;
using System.Net.Http.Json;
using DigitalProject.Tests.Helpers;
using FluentAssertions;

namespace DigitalProject.Tests.Admin
{
    [Collection("Integration Tests")]
    public class AdminTests
    {
        private readonly HttpClient _adminClient;
        private readonly HttpClient _userClient;
        private readonly CustomWebApplicationFactory _factory;

        public AdminTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _adminClient = factory.CreateAdminClient();

            // 建立一般用戶
            _userClient = factory.CreateClientWithCookies();
            _userClient.PostAsJsonAsync("/api/auth/register", new
            {
                email = "normaluser@test.com",
                password = "Test1234",
                displayName = "一般用戶"
            }).Wait();
            _userClient.PostAsJsonAsync("/api/auth/login", new
            {
                email = "normaluser@test.com",
                password = "Test1234"
            }).Wait();
        }

        // ── 統計 API ────────────────────────────────────────────

        [Fact]
        public async Task GetStats_WithAdmin_ReturnsOk()
        {
            var response = await _adminClient.GetAsync("/api/admin/stats");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetStats_WithNormalUser_Returns403()
        {
            var response = await _userClient.GetAsync("/api/admin/stats");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetStats_WithoutLogin_Returns401()
        {
            var anonClient = _factory.CreateClient();
            var response = await anonClient.GetAsync("/api/admin/stats");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── 商品管理 ────────────────────────────────────────────

        [Fact]
        public async Task AdminGetProducts_WithAdmin_ReturnsOk()
        {
            var response = await _adminClient.GetAsync("/api/admin/product");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminGetProducts_WithNormalUser_Returns403()
        {
            var response = await _userClient.GetAsync("/api/admin/product");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AdminCreateProduct_WithAdmin_ReturnsOk()
        {
            var response = await _adminClient.PostAsJsonAsync("/api/admin/product", new
            {
                name = "測試新增商品",
                description = "測試描述",
                price = 199,
                categoryId = "00000000-0000-0000-0000-000000000030",
                isPublished = true
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminCreateProduct_WithNormalUser_Returns403()
        {
            var response = await _userClient.PostAsJsonAsync("/api/admin/product", new
            {
                name = "惡意商品",
                price = 199,
                categoryId = "00000000-0000-0000-0000-000000000030",
                isPublished = true
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── 訂單管理 ────────────────────────────────────────────

        [Fact]
        public async Task AdminGetOrders_WithAdmin_ReturnsOk()
        {
            var response = await _adminClient.GetAsync("/api/admin/order");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminGetOrders_WithNormalUser_Returns403()
        {
            var response = await _userClient.GetAsync("/api/admin/order");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── 用戶管理 ────────────────────────────────────────────

        [Fact]
        public async Task AdminGetUsers_WithAdmin_ReturnsOk()
        {
            var response = await _adminClient.GetAsync("/api/admin/user");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminGetUsers_WithNormalUser_Returns403()
        {
            var response = await _userClient.GetAsync("/api/admin/user");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── 付款管理 ────────────────────────────────────────────

        [Fact]
        public async Task AdminGetPayments_WithAdmin_ReturnsOk()
        {
            var response = await _adminClient.GetAsync("/api/admin/payment");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminGetPayments_WithNormalUser_Returns403()
        {
            var response = await _userClient.GetAsync("/api/admin/payment");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── 評論管理 ────────────────────────────────────────────

        [Fact]
        public async Task AdminGetReviews_WithAdmin_ReturnsOk()
        {
            var response = await _adminClient.GetAsync("/api/admin/review");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminGetReviews_WithNormalUser_Returns403()
        {
            var response = await _userClient.GetAsync("/api/admin/review");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}