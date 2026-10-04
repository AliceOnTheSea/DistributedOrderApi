using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using DistributedOrderApi.Application.Orders.Queries;
using FluentAssertions;
using Moq;
using Xunit;

namespace DistributedOrderApi.UnitTests.Application.Orders.Queries;

public class NaturalLanguageSearchQueryHandlerTests
{
    private readonly Mock<ILlmSearchFilterService> _llmServiceMock;
    private readonly Mock<IOrderReadRepository> _repositoryMock;
    private readonly NaturalLanguageSearchQueryHandler _handler;

    public NaturalLanguageSearchQueryHandlerTests()
    {
        _llmServiceMock = new Mock<ILlmSearchFilterService>();
        _repositoryMock = new Mock<IOrderReadRepository>();
        _handler = new NaturalLanguageSearchQueryHandler(_llmServiceMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsNaturalSearchResponse()
    {
        // Arrange
        var query = new NaturalLanguageSearchQuery("completed orders over $100");
        var expectedFilter = new OrderSearchFilter(Status: "Completed", MinAmount: 100m);
        var expectedExplanation = "Showing completed orders > $100.";
        var expectedOrders = new List<OrderDto>
        {
            new OrderDto(
                Guid.NewGuid(), "CUST-01", "Alice", "alice@example.com",
                new AddressDto("123 St", "City", "ST", "12345", "USA"),
                "Completed", "Paid", 200m, "USD", DateTime.UtcNow, DateTime.UtcNow, new List<OrderItemDto>())
        };

        _llmServiceMock
            .Setup(x => x.TranslateQueryAsync(query.Query, It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedFilter, expectedExplanation));

        _repositoryMock
            .Setup(x => x.SearchOrdersAsync(expectedFilter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Explanation.Should().Be(expectedExplanation);
        result.Filter.Should().Be(expectedFilter);
        result.Orders.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyQuery_ThrowsArgumentException()
    {
        // Arrange
        var query = new NaturalLanguageSearchQuery("");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
