using DistributedOrderApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DistributedOrderApi.Application.Orders.Commands;

public record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<bool>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventDispatcher _eventDispatcher;

    public CancelOrderCommandHandler(IApplicationDbContext context, IEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            throw new KeyNotFoundException($"Order with ID '{request.OrderId}' was not found.");

        order.CancelOrder(request.Reason);

        await _context.SaveChangesAsync(cancellationToken);
        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return true;
    }
}
