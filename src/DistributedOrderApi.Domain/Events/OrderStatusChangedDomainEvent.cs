using DistributedOrderApi.Domain.Common;
using DistributedOrderApi.Domain.Enums;

namespace DistributedOrderApi.Domain.Events;

public record OrderStatusChangedDomainEvent(
    Guid OrderId,
    OrderStatus PreviousStatus,
    OrderStatus NewStatus,
    DateTime OccurredOnUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}
