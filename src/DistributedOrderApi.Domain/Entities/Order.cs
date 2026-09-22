using DistributedOrderApi.Domain.Common;
using DistributedOrderApi.Domain.Enums;
using DistributedOrderApi.Domain.Events;
using DistributedOrderApi.Domain.Exceptions;
using DistributedOrderApi.Domain.ValueObjects;

namespace DistributedOrderApi.Domain.Entities;

public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = new();

    public CustomerInfo Customer { get; private set; }
    public Address ShippingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public Money TotalAmount { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order()
    {
        Customer = null!;
        ShippingAddress = null!;
        Status = OrderStatus.Draft;
        PaymentStatus = PaymentStatus.Pending;
        TotalAmount = Money.Zero();
    }

    public Order(CustomerInfo customer, Address shippingAddress, string currency = "USD")
    {
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        Status = OrderStatus.Draft;
        PaymentStatus = PaymentStatus.Pending;
        TotalAmount = Money.Zero(currency);

        AddDomainEvent(new OrderCreatedDomainEvent(
            Id,
            Customer.Email,
            TotalAmount.Amount,
            TotalAmount.Currency,
            DateTime.UtcNow));
    }

    public void AddItem(string productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.Submitted)
            throw new InvalidOrderStateException($"Cannot modify order items when order is in '{Status}' state.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = new OrderItem(Id, productId, productName, unitPrice, quantity);
            _items.Add(newItem);
        }

        RecalculateTotal();
        Touch();
    }

    public void RemoveItem(string productId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOrderStateException($"Cannot remove items when order status is '{Status}'.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotal();
            Touch();
        }
    }

    public void SubmitOrder()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOrderStateException(Status, OrderStatus.Submitted);

        if (!_items.Any())
            throw new DomainException("Cannot submit an empty order without items.");

        var previousStatus = Status;
        Status = OrderStatus.Submitted;
        Touch();

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, previousStatus, Status, DateTime.UtcNow));
    }

    public void MarkProcessing()
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOrderStateException(Status, OrderStatus.Processing);

        var previousStatus = Status;
        Status = OrderStatus.Processing;
        Touch();

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, previousStatus, Status, DateTime.UtcNow));
    }

    public void CompleteOrder()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOrderStateException(Status, OrderStatus.Completed);

        var previousStatus = Status;
        Status = OrderStatus.Completed;
        PaymentStatus = PaymentStatus.Captured;
        Touch();

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, previousStatus, Status, DateTime.UtcNow));
    }

    public void CancelOrder(string reason)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new InvalidOrderStateException($"Cannot cancel order in '{Status}' state.");

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        Touch();

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, previousStatus, Status, DateTime.UtcNow));
    }

    private void RecalculateTotal()
    {
        var currency = TotalAmount?.Currency ?? "USD";
        decimal total = _items.Sum(item => item.Subtotal.Amount);
        TotalAmount = new Money(total, currency);
    }
}
