using DistributedOrderApi.Domain.Common;
using DistributedOrderApi.Domain.Enums;

namespace DistributedOrderApi.Domain.Events;

public record OrderCreatedDomainEvent(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount,
    string Currency,
    DateTime OccurredOnUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}
