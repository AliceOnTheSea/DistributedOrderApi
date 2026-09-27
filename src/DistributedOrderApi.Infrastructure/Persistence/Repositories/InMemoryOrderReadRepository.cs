using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using DistributedOrderApi.Domain.Entities;
using DistributedOrderApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DistributedOrderApi.Infrastructure.Persistence.Repositories;

public class InMemoryOrderReadRepository : IOrderReadRepository
{
    private readonly OrderDbContext _dbContext;

    public InMemoryOrderReadRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order != null ? MapToDto(order) : null;
    }

    public async Task<IReadOnlyList<OrderDto>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.Customer.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderSummaryDto> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        int totalOrders = orders.Count;
        int pendingOrders = orders.Count(o => o.Status == OrderStatus.Draft || o.Status == OrderStatus.Submitted || o.Status == OrderStatus.Processing);
        int completedOrders = orders.Count(o => o.Status == OrderStatus.Completed);
        int cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
        decimal totalRevenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount.Amount);

        return new OrderSummaryDto(totalOrders, pendingOrders, completedOrders, cancelledOrders, totalRevenue);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.Customer.CustomerId,
            order.Customer.FullName,
            order.Customer.Email,
            new AddressDto(
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.ZipCode,
                order.ShippingAddress.Country),
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice.Amount,
                i.UnitPrice.Currency,
                i.Quantity,
                i.Subtotal.Amount)).ToList());
    }
}
