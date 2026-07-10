// AuthTests.cs
using System.Net;
using System.Net.Http.Json;
using DigitalProject.Tests.Helpers;
using FluentAssertions;

namespace DigitalProject.Tests.Auth
{
    [Collection("Integration Tests")]
    public class AuthTests
    {
        private readonly HttpClient _client;

        public AuthTests(CustomWebApplicationFactory factory)
        {
            // ← 改用帶 Cookie 的 Client
            _client = factory.CreateClientWithCookies();

            // 建立測試帳號
            _client.PostAsJsonAsync("/api/auth/register", new
            {
                email = "test@test.com",
                password = "Test1234",
                displayName = "測試用戶"
            }).Wait();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var request = new { email = "test@test.com", password = "Test1234" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            var request = new { email = "test@test.com", password = "wrongpassword" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_Returns401()
        {
            var request = new { email = "notexist@test.com", password = "Test1234" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Register_WithNewEmail_ReturnsOk()
        {
            // 用不同的 Email 避免跟建構子衝突
            var request = new
            {
                email = "newuser123@test.com",
                password = "Test1234",
                displayName = "新用戶"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Register_WithExistingEmail_Returns400()
        {
            // test@test.com 已在建構子建立
            var request = new
            {
                email = "test@test.com",
                password = "Test1234",
                displayName = "重複用戶"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Logout_WithValidToken_ReturnsOk()
        {
            // 先登入
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "test@test.com",
                password = "Test1234"
            });

            // 印出登入結果和 Cookie
            var loginStatus = loginResponse.StatusCode;
            var cookies = loginResponse.Headers.ToString();
            Console.WriteLine($"Login Status: {loginStatus}");
            Console.WriteLine($"Response Headers: {cookies}");

            // 登出
            var response = await _client.PostAsJsonAsync("/api/auth/logout", new { });
            Console.WriteLine($"Logout Status: {response.StatusCode}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}