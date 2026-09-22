using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using DistributedOrderApi.Domain.Entities;
using DistributedOrderApi.Domain.ValueObjects;
using MediatR;

namespace DistributedOrderApi.Application.Orders.Commands;

public record CreateOrderCommand(
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    AddressDto ShippingAddress,
    string Currency,
    List<CreateOrderItemRequest> Items) : IRequest<Guid>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IVendorCatalogClient _vendorCatalogClient;
    private readonly IEventDispatcher _eventDispatcher;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        IVendorCatalogClient vendorCatalogClient,
        IEventDispatcher eventDispatcher)
    {
        _context = context;
        _vendorCatalogClient = vendorCatalogClient;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = new CustomerInfo(request.CustomerId, request.CustomerName, request.CustomerEmail);
        var address = new Address(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.ZipCode,
            request.ShippingAddress.Country);

        var order = new Order(customer, address, request.Currency);

        foreach (var item in request.Items)
        {
            // Resilient vendor check via Polly client
            var stockAvailable = await _vendorCatalogClient.ConfirmVendorStockAsync(item.ProductId, item.Quantity, cancellationToken);
            if (!stockAvailable)
            {
                throw new InvalidOperationException($"Vendor stock insufficient for product ID: {item.ProductId}");
            }

            var money = new Money(item.UnitPrice, request.Currency);
            order.AddItem(item.ProductId, item.ProductName, money, item.Quantity);
        }

        order.SubmitOrder();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return order.Id;
    }
}
