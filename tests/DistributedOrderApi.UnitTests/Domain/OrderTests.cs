using DistributedOrderApi.Domain.Entities;
using DistributedOrderApi.Domain.Enums;
using DistributedOrderApi.Domain.Events;
using DistributedOrderApi.Domain.Exceptions;
using DistributedOrderApi.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DistributedOrderApi.UnitTests.Domain;

public class OrderTests
{
    private readonly CustomerInfo _sampleCustomer = new("CUST-100", "Jane Doe", "jane.doe@example.com");
    private readonly Address _sampleAddress = new("123 Market St", "Seattle", "WA", "98101", "USA");

    [Fact]
    public void Order_Creation_Should_Initialize_In_Draft_State_And_Emit_Event()
    {
        // Act
        var order = new Order(_sampleCustomer, _sampleAddress, "USD");

        // Assert
        order.Status.Should().Be(OrderStatus.Draft);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        order.TotalAmount.Amount.Should().Be(0);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCreatedDomainEvent);
    }

    [Fact]
    public void AddItem_Should_Increase_Total_Amount_And_Add_To_Items()
    {
        // Arrange
        var order = new Order(_sampleCustomer, _sampleAddress, "USD");
        var unitPrice = new Money(99.99m, "USD");

        // Act
        order.AddItem("PROD-1", "Wireless Keyboard", unitPrice, 2);

        // Assert
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(199.98m);
    }

    [Fact]
    public void SubmitOrder_Should_Transition_Status_To_Submitted()
    {
        // Arrange
        var order = new Order(_sampleCustomer, _sampleAddress, "USD");
        order.AddItem("PROD-1", "Wireless Keyboard", new Money(50.00m, "USD"), 1);

        // Act
        order.SubmitOrder();

        // Assert
        order.Status.Should().Be(OrderStatus.Submitted);
        order.DomainEvents.Should().Contain(e => e is OrderStatusChangedDomainEvent);
    }

    [Fact]
    public void SubmitOrder_EmptyItems_Should_Throw_DomainException()
    {
        // Arrange
        var order = new Order(_sampleCustomer, _sampleAddress, "USD");

        // Act & Assert
        order.Invoking(o => o.SubmitOrder())
            .Should().Throw<DomainException>()
            .WithMessage("Cannot submit an empty order without items.");
    }

    [Fact]
    public void CancelOrder_WhenCompleted_Should_Throw_InvalidOrderStateException()
    {
        // Arrange
        var order = new Order(_sampleCustomer, _sampleAddress, "USD");
        order.AddItem("PROD-1", "Item", new Money(10m, "USD"), 1);
        order.SubmitOrder();
        order.MarkProcessing();
        order.CompleteOrder();

        // Act & Assert
        order.Invoking(o => o.CancelOrder("Changed mind"))
            .Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot cancel order in 'Completed' state.");
    }
}
