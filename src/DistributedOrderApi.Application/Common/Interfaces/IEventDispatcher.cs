using DistributedOrderApi.Domain.Common;

namespace DistributedOrderApi.Application.Common.Interfaces;

public interface IEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
