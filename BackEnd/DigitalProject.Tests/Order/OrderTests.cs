// DigitalProject.Tests/Order/OrderTests.cs
using System.Net;
using System.Net.Http.Json;
using DigitalProject.Tests.Helpers;
using FluentAssertions;

namespace DigitalProject.Tests.Order
{
    [Collection("Integration Tests")]
    public class OrderTests
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;  



        public OrderTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClientWithCookies();

            // 建立測試帳號並登入
            _client.PostAsJsonAsync("/api/auth/register", new
            {
                email = "ordertest@test.com",
                password = "Test1234",
                displayName = "訂單測試用戶"
            }).Wait();

            _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "ordertest@test.com",
                password = "Test1234"
            }).Wait();
        }

        // ── 未登入測試 ──────────────────────────────────────────

        [Fact]
        public async Task GetOrders_WithoutLogin_Returns401()
        {
            // 建立未登入的 Client
            var anonClient = _factory.CreateClient();  // ← 不帶 Cookie
            var response = await anonClient.GetAsync("/api/order");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── 訂單列表 ────────────────────────────────────────────

        [Fact]
        public async Task GetOrders_WithLogin_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/order");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetOrders_ReturnsPagedResponse()
        {
            var response = await _client.GetAsync("/api/order?page=1&pageSize=10");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrEmpty();
            body.Should().Contain("data");      // 確認有 data 欄位
            body.Should().Contain("total");     // 確認有 total 欄位
            body.Should().Contain("totalPages"); // 確認有 totalPages 欄位
        }

        // ── 結帳測試 ────────────────────────────────────────────

        [Fact]
        public async Task Checkout_CreditCard_Success_ReturnsOk()
        {
            var productId = "00000000-0000-0000-0000-000000000040";

            var response = await _client.PostAsJsonAsync("/api/payment/checkout", new
            {
                productIds = new[] { productId },
                provider = 2,  // CreditCard
                cardNumber = "4111111111111111",
                cardHolder = "Test User",
                expiryDate = "12/28",
                cvv = "123"
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Checkout_CreditCard_Fail_Returns402()
        {
            var productId = "00000000-0000-0000-0000-000000000040";

            var response = await _client.PostAsJsonAsync("/api/payment/checkout", new
            {
                productIds = new[] { productId },
                provider = 2,  // CreditCard
                cardNumber = "1111111100000000",  // ← 0000 結尾 → 失敗
                cardHolder = "Test User",
                expiryDate = "12/28",
                cvv = "123"
            });

            response.StatusCode.Should().Be(HttpStatusCode.PaymentRequired);  // 402
        }

        [Fact]
        public async Task Checkout_CVS_ReturnsOk()
        {
            var productId = "00000000-0000-0000-0000-000000000040";

            var response = await _client.PostAsJsonAsync("/api/payment/checkout", new
            {
                productIds = new[] { productId },
                provider = 3  // CVS
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ── 取消訂單 ────────────────────────────────────────────

        [Fact]
        public async Task CancelOrder_CVSPending_ReturnsOk()
        {
            var productId = "00000000-0000-0000-0000-000000000040";

            // 1. 建立超商訂單
            var checkoutResponse = await _client.PostAsJsonAsync("/api/payment/checkout", new
            {
                productIds = new[] { productId },
                provider = 3  // CVS
            });
            checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var checkout = await checkoutResponse.Content
                .ReadFromJsonAsync<CheckoutResponseDto>();

            // 2. 取消訂單
            var cancelResponse = await _client.PutAsJsonAsync(
                $"/api/order/{checkout!.OrderId}/cancel", new { });

            cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CancelOrder_PaidOrder_ReturnsBadRequest()
        {
            var productId = "00000000-0000-0000-0000-000000000040";

            // 1. 信用卡結帳（status = Paid）
            var checkoutResponse = await _client.PostAsJsonAsync("/api/payment/checkout", new
            {
                productIds = new[] { productId },
                provider = 2,
                cardNumber = "4111111111111111",
                cardHolder = "Test User",
                expiryDate = "12/28",
                cvv = "123"
            });

            var checkout = await checkoutResponse.Content
                .ReadFromJsonAsync<CheckoutResponseDto>();

            // 2. 嘗試取消已付款訂單
            var cancelResponse = await _client.PutAsJsonAsync(
                $"/api/order/{checkout!.OrderId}/cancel", new { });

            cancelResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetOrderById_NotOwner_Returns403()
        {
            var productId = "00000000-0000-0000-0000-000000000040";

            // 1. 用測試帳號建立訂單
            var checkoutResponse = await _client.PostAsJsonAsync("/api/payment/checkout", new
            {
                productIds = new[] { productId },
                provider = 3
            });
            var checkout = await checkoutResponse.Content
                .ReadFromJsonAsync<CheckoutResponseDto>();

            // 2. 建立另一個帳號
            var otherClient = _factory.CreateClientWithCookies();
            await otherClient.PostAsJsonAsync("/api/auth/register", new
            {
                email = "other@test.com",
                password = "Test1234",
                displayName = "其他用戶"
            });
            await otherClient.PostAsJsonAsync("/api/auth/login", new
            {
                email = "other@test.com",
                password = "Test1234"
            });

            // 3. 用其他帳號查詢訂單
            var response = await otherClient.GetAsync($"/api/order/{checkout!.OrderId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    // ── 本地 DTO，只給測試用 ──────────────────────────────────
    public class CheckoutResponseDto
    {
        public string OrderNo { get; set; } = null!;
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentCode { get; set; }
    }
}