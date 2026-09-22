using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using DistributedOrderApi.Application.Orders.Commands;
using DistributedOrderApi.Domain.Common;
using DistributedOrderApi.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DistributedOrderApi.UnitTests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IVendorCatalogClient> _mockVendorClient;
    private readonly Mock<IEventDispatcher> _mockDispatcher;
    private readonly Mock<DbSet<Order>> _mockDbSet;

    public CreateOrderCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockVendorClient = new Mock<IVendorCatalogClient>();
        _mockDispatcher = new Mock<IEventDispatcher>();
        _mockDbSet = new Mock<DbSet<Order>>();

        _mockContext.Setup(c => c.Orders).Returns(_mockDbSet.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_Should_Create_Order_And_Return_Guid()
    {
        // Arrange
        _mockVendorClient
            .Setup(v => v.ConfirmVendorStockAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateOrderCommand(
            "CUST-001",
            "John Doe",
            "john.doe@example.com",
            new AddressDto("100 Main St", "Austin", "TX", "78701", "USA"),
            "USD",
            new List<CreateOrderItemRequest>
            {
                new("PROD-100", "Gaming Mouse", 79.99m, 1)
            });

        var handler = new CreateOrderCommandHandler(_mockContext.Object, _mockVendorClient.Object, _mockDispatcher.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockDbSet.Verify(m => m.Add(It.IsAny<Order>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_VendorOutOfStock_Should_Throw_InvalidOperationException()
    {
        // Arrange
        _mockVendorClient
            .Setup(v => v.ConfirmVendorStockAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateOrderCommand(
            "CUST-001",
            "John Doe",
            "john.doe@example.com",
            new AddressDto("100 Main St", "Austin", "TX", "78701", "USA"),
            "USD",
            new List<CreateOrderItemRequest>
            {
                new("PROD-100", "Gaming Mouse", 79.99m, 1)
            });

        var handler = new CreateOrderCommandHandler(_mockContext.Object, _mockVendorClient.Object, _mockDispatcher.Object);

        // Act & Assert
        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Vendor stock insufficient*");
    }
}
