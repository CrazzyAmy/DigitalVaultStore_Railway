using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DigitalProject.Tests.Product
{
    [Collection("Integration Tests")]
    public class ProductTests
    {
        private readonly HttpClient _client;
        public ProductTests(Helpers.CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClientWithCookies();
        }
        [Fact]
        public async Task GetAll_ReturnOk()
        {
            var response = await _client.GetAsync("/api/product");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Fact]
        public async Task GetAll_WithKeyword_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/product?keyword=測試");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_WithCategoryId_ReturnsOk()
        {
            var categoryId = "00000000-0000-0000-0000-000000000030";
            var response = await _client.GetAsync($"/api/product?categoryId={categoryId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_WithPriceRange_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/product?minPrice=0&maxPrice=200");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_WithSortByPrice_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/product?sortBy=price&sortOrder=asc");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOk()
        {
            var productId = "00000000-0000-0000-0000-000000000040";
            var response = await _client.GetAsync($"/api/product/{productId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            var response = await _client.GetAsync($"/api/product/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        [Fact]
        public async Task GetById_UnpublishedProduct_ReturnsNotFound()
        {
            // 未上架商品不應該在前台顯示
            var unpublishedId = "00000000-0000-0000-0000-000000000041";
            var response = await _client.GetAsync($"/api/product/{unpublishedId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
