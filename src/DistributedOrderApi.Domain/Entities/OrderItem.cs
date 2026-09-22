using DistributedOrderApi.Domain.Common;
using DistributedOrderApi.Domain.Exceptions;
using DistributedOrderApi.Domain.ValueObjects;

namespace DistributedOrderApi.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public string ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Money Subtotal => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);

    private OrderItem()
    {
        ProductId = string.Empty;
        ProductName = string.Empty;
        UnitPrice = Money.Zero();
    }

    public OrderItem(Guid orderId, string productId, string productName, Money unitPrice, int quantity)
    {
        if (orderId == Guid.Empty) throw new DomainException("Order ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(productId)) throw new DomainException("Product ID is required.");
        if (string.IsNullOrWhiteSpace(productName)) throw new DomainException("Product Name is required.");
        if (unitPrice == null) throw new ArgumentNullException(nameof(unitPrice));
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");

        OrderId = orderId;
        ProductId = productId.Trim();
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        Quantity = newQuantity;
        Touch();
    }
}
