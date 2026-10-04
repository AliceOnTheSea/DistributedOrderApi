using DistributedOrderApi.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace DistributedOrderApi.UnitTests.Infrastructure.Services;

public class MockLlmSearchFilterServiceTests
{
    private readonly MockLlmSearchFilterService _service;

    public MockLlmSearchFilterServiceTests()
    {
        _service = new MockLlmSearchFilterService();
    }

    [Fact]
    public async Task TranslateQueryAsync_DelayedOrdersOver500_ParsesFilterAndExplanation()
    {
        // Act
        var (filter, explanation) = await _service.TranslateQueryAsync("all delayed orders over $500 from last week");

        // Assert
        filter.Should().NotBeNull();
        filter.Status.Should().Be("Processing");
        filter.MinAmount.Should().Be(500m);
        filter.StartDate.Should().NotBeNull();
        explanation.Should().Contain("Processing");
        explanation.Should().Contain("500");
    }

    [Fact]
    public async Task TranslateQueryAsync_CompletedOrdersForCustomer_ParsesCustomerId()
    {
        // Act
        var (filter, explanation) = await _service.TranslateQueryAsync("completed orders for customer CUST-100");

        // Assert
        filter.Status.Should().Be("Completed");
        filter.CustomerId.Should().Be("CUST-100");
        explanation.Should().Contain("CUST-100");
    }
}
