using DistributedOrderApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DistributedOrderApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
