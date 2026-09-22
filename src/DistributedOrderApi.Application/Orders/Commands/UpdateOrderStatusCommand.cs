using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Domain.Enums;
using DistributedOrderApi.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DistributedOrderApi.Application.Orders.Commands;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus TargetStatus) : IRequest<bool>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventDispatcher _eventDispatcher;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context, IEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            throw new KeyNotFoundException($"Order with ID '{request.OrderId}' was not found.");

        switch (request.TargetStatus)
        {
            case OrderStatus.Processing:
                order.MarkProcessing();
                break;
            case OrderStatus.Completed:
                order.CompleteOrder();
                break;
            default:
                throw new InvalidOrderStateException($"Invalid target status transition to {request.TargetStatus}");
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return true;
    }
}
