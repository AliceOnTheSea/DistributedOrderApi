using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DistributedOrderApi.Infrastructure.Services;

public class EventDispatcher : IEventDispatcher
{
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(ILogger<EventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var @event in domainEvents)
        {
            _logger.LogInformation("Dispatched Domain Event {EventName} [ID: {EventId}] occurred at {OccurredOn}",
                @event.GetType().Name, @event.EventId, @event.OccurredOnUtc);
        }

        return Task.CompletedTask;
    }
}
