using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using DistributedOrderApi.Domain.Entities;
using DistributedOrderApi.Domain.Enums;
using DistributedOrderApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DistributedOrderApi.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real SQL Server DbContext
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestOrderDb");
            });

            // Replace Dapper SQL Repository with In-Memory Test Read Repository
            var readRepoDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IOrderReadRepository));
            if (readRepoDescriptor != null)
            {
                services.Remove(readRepoDescriptor);
            }

            services.AddScoped<IOrderReadRepository, TestOrderReadRepository>();

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<OrderDbContext>();

            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development");
    }
}

public class TestOrderReadRepository : IOrderReadRepository
{
    private readonly OrderDbContext _dbContext;

    public TestOrderReadRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order != null ? MapToDto(order) : null;
    }

    public async Task<IReadOnlyList<OrderDto>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.Customer.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderSummaryDto> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders.ToListAsync(cancellationToken);

        int total = orders.Count;
        int pending = orders.Count(o => o.Status == OrderStatus.Draft || o.Status == OrderStatus.Submitted || o.Status == OrderStatus.Processing);
        int completed = orders.Count(o => o.Status == OrderStatus.Completed);
        int cancelled = orders.Count(o => o.Status == OrderStatus.Cancelled);
        decimal revenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount.Amount);

        return new OrderSummaryDto(total, pending, completed, cancelled, revenue);
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
