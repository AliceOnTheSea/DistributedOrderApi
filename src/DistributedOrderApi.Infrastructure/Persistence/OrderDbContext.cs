using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DistributedOrderApi.Infrastructure.Persistence;

public class OrderDbContext : DbContext, IApplicationDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
