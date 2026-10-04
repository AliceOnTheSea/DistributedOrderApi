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
            .Where(o => o.Customer != null && o.Customer.CustomerId == customerId)
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
        decimal totalRevenue = orders.Where(o => o.Status == OrderStatus.Completed && o.TotalAmount != null).Sum(o => o.TotalAmount.Amount);

        return new OrderSummaryDto(totalOrders, pendingOrders, completedOrders, cancelledOrders, totalRevenue);
    }

    public async Task<IReadOnlyList<OrderDto>> SearchOrdersAsync(OrderSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<OrderStatus>(filter.Status, true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (filter.MinAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount != null && o.TotalAmount.Amount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount != null && o.TotalAmount.Amount <= filter.MaxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerId))
        {
            query = query.Where(o => o.Customer != null && o.Customer.CustomerId == filter.CustomerId);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAtUtc >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAtUtc <= filter.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
        {
            var kw = filter.SearchKeyword.ToLower();
            query = query.Where(o =>
                (o.Customer != null && (o.Customer.FullName.ToLower().Contains(kw) || o.Customer.Email.ToLower().Contains(kw))) ||
                o.Items.Any(i => i.ProductName.ToLower().Contains(kw) || i.ProductId.ToLower().Contains(kw)));
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.Customer != null ? order.Customer.CustomerId : string.Empty,
            order.Customer != null ? order.Customer.FullName : string.Empty,
            order.Customer != null ? order.Customer.Email : string.Empty,
            order.ShippingAddress != null
                ? new AddressDto(
                    order.ShippingAddress.Street,
                    order.ShippingAddress.City,
                    order.ShippingAddress.State,
                    order.ShippingAddress.ZipCode,
                    order.ShippingAddress.Country)
                : new AddressDto(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty),
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.TotalAmount != null ? order.TotalAmount.Amount : 0m,
            order.TotalAmount != null ? order.TotalAmount.Currency : "USD",
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            order.Items != null
                ? order.Items.Select(i => new OrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice != null ? i.UnitPrice.Amount : 0m,
                    i.UnitPrice != null ? i.UnitPrice.Currency : "USD",
                    i.Quantity,
                    i.Subtotal != null ? i.Subtotal.Amount : 0m)).ToList()
                : new List<OrderItemDto>());
    }
}
