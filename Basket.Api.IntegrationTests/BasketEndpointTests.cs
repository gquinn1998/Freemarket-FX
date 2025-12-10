using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Basket.Api.IntegrationTests;

[TestClass]
public class BasketEndpointTests
{
    private static BasketApiFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static void Init(TestContext _)
    {
        _factory = new BasketApiFactory();
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task GetTotalShouldReturn200()
    {
        // Arrange
        var basketId = Guid.NewGuid();

        // Add item
        await _client.PostAsJsonAsync(
            $"/api/v1/basket/{basketId}/items",
            new { Name = "Apple", Price = 2.00m, Quantity = 2 }
        );

        // Act
        var response = await _client.GetAsync($"/api/v1/basket/{basketId}/totals");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task AddItemShouldReturn201()
    {
        var basketId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/basket/{basketId}/items",
            new { Name = "Orange", Price = 1.5m, Quantity = 1 }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
