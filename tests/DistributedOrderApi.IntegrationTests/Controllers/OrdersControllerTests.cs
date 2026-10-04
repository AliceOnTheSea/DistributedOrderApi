using System.Net;
using System.Net.Http.Json;
using DistributedOrderApi.Application.Dtos;
using DistributedOrderApi.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace DistributedOrderApi.IntegrationTests.Controllers;

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_CreateOrder_ValidPayload_Returns_Created()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: "CUST-INT-01",
            CustomerName: "Alice Integration",
            CustomerEmail: "alice.int@example.com",
            ShippingAddress: new AddressDto("456 Innovation Way", "San Francisco", "CA", "94105", "USA"),
            Currency: "USD",
            Items: new List<CreateOrderItemRequest>
            {
                new("PROD-404", "Developer Mechanical Keyboard", 149.99m, 1)
            });

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        content.Should().ContainKey("id");
        content!["id"].Should().NotBeEmpty();
    }

    [Fact]
    public async Task Get_OrderById_SeededSubmittedOrder_Returns_200OK()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{DemoDataSeeder.DemoOrderSubmittedId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Id.Should().Be(DemoDataSeeder.DemoOrderSubmittedId);
        order.Status.Should().Be("Submitted");
        order.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Get_OrderSummary_Returns_SuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/orders/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_NaturalLanguageSearch_ValidQuery_Returns_200OK_WithParsedFilter()
    {
        // Arrange
        var request = new NaturalSearchRequest("orders over $200");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders/search/natural", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<NaturalSearchResponse>();
        result.Should().NotBeNull();
        result!.Explanation.Should().NotBeNullOrEmpty();
        result.Filter.Should().NotBeNull();
        result.Filter.MinAmount.Should().Be(200m);
        result.Orders.Should().NotBeNull();
    }
}
