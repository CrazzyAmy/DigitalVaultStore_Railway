using DigitalProject.Tests.Helpers;
using DigitalProject.Tests.Order;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DigitalProject.Tests.Review
{
    [Collection("Integration Tests")]
    public class ReviewTests
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;
        private readonly string _productId = "00000000-0000-0000-0000-000000000040";

        public ReviewTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClientWithCookies();

            // 建立測試帳號並登入
            _client.PostAsJsonAsync("/api/auth/register", new
            {
                email = "review@test.com",
                password = "Test1234",
                displayName = "評論測試用戶"
            }).Wait();

            _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "review@test.com",
                password = "Test1234"
            }).Wait();
        }

        // ── 取得評論（公開） ────────────────────────────────────

        [Fact]
        public async Task GetReviewsByProduct_ReturnsOk()
        {
            var response = await _client.GetAsync($"/api/review/product/{_productId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetReviewStats_ReturnsOk()
        {
            var response = await _client.GetAsync($"/api/review/product/{_productId}/stats");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetReviewsByProduct_WithoutLogin_ReturnsOk()
        {
            // 未登入也可以看評論
            var anonClient = _factory.CreateClient();
            var response = await anonClient.GetAsync($"/api/review/product/{_productId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ── 新增評論（需已購買） ────────────────────────────────

        [Fact]
        public async Task CreateReview_WithoutPurchase_Returns403()
        {
            var response = await _client.PostAsJsonAsync("/api/review", new
            {
                productId = _productId,
                orderId = Guid.NewGuid(),
                rating = 5,
                comment = "測試評論"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateReview_AfterPurchase_ReturnsOk()
        {
            // 1. 先購買商品
            var checkoutResponse = await _client.PostAsJsonAsync("/api/payment/checkout", new
            {
                productIds = new[] { _productId },
                provider = 2,
                cardNumber = "4111111111111111",
                cardHolder = "Test User",
                expiryDate = "12/28",
                cvv = "123"
            });
            checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var checkout = await checkoutResponse.Content
                .ReadFromJsonAsync<CheckoutResponseDto>();

            // 2. 新增評論
            var response = await _client.PostAsJsonAsync("/api/review", new
            {
                productId = _productId,
                orderId = checkout!.OrderId,
                rating = 5,
                comment = "很棒的商品"
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateReview_DuplicateOrder_ReturnsError()
        {
            // 1. 購買並評論
            var checkoutResponse = await _client.PostAsJsonAsync("/api/payment/checkout", new
            {
                productIds = new[] { _productId },
                provider = 2,
                cardNumber = "4111111111111111",
                cardHolder = "Test User",
                expiryDate = "12/28",
                cvv = "123"
            });

            var checkout = await checkoutResponse.Content
                .ReadFromJsonAsync<CheckoutResponseDto>();

            await _client.PostAsJsonAsync("/api/review", new
            {
                productId = _productId,
                orderId = checkout!.OrderId,
                rating = 5,
                comment = "第一次評論"
            });

            // 2. 同一訂單再次評論 → 應該失敗
            var response = await _client.PostAsJsonAsync("/api/review", new
            {
                productId = _productId,
                orderId = checkout.OrderId,
                rating = 4,
                comment = "第二次評論"
            });

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateReview_WithoutLogin_Returns401()
        {
            var anonClient = _factory.CreateClient();

            var response = await anonClient.PostAsJsonAsync("/api/review", new
            {
                productId = _productId,
                orderId = Guid.NewGuid(),
                rating = 5,
                comment = "測試評論"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── 我的評論 ────────────────────────────────────────────

        [Fact]
        public async Task GetMyReviews_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/review/my");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetMyReviews_WithoutLogin_Returns401()
        {
            var anonClient = _factory.CreateClient();
            var response = await anonClient.GetAsync("/api/review/my");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}