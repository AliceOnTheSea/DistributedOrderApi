namespace DistributedOrderApi.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DistributedOrderApi.Domain.Entities;
using DistributedOrderApi.Domain.ValueObjects;

public class DemoDataSeeder
{
    private readonly OrderDbContext _dbContext;

    public DemoDataSeeder(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public static readonly Guid DemoOrderSubmittedId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoOrderProcessingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid DemoOrderCompletedId  = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public async Task SeedAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();

        if (await _dbContext.Orders.AnyAsync())
        {
            return;
        }

        var customer = new CustomerInfo("Jane Doe", "jane.doe@example.com");
        var address = new Address("123 Market St", "Portland", "OR", "97201", "USA");

        // 1. Submitted Order
        var order1 = Order.Create(DemoOrderSubmittedId, customer, address);
        order1.AddLineItem("SKU-PRO-01", "Mechanical Keyboard", 1, new Money(149.99m, "USD"));
        order1.AddLineItem("SKU-ACC-02", "USB-C Braided Cable", 2, new Money(14.50m, "USD"));
        order1.Submit();

        // 2. Processing Order
        var order2 = Order.Create(DemoOrderProcessingId, customer, address);
        order2.AddLineItem("SKU-DISP-09", "4K Ultra-Wide Monitor", 1, new Money(499.00m, "USD"));
        order2.Submit();
        order2.StartProcessing();

        // 3. Completed Order
        var order3 = Order.Create(DemoOrderCompletedId, customer, address);
        order3.AddLineItem("SKU-AUD-04", "Active Noise Cancelling Headphones", 1, new Money(229.00m, "USD"));
        order3.Submit();
        order3.StartProcessing();
        order3.Complete();

        await _dbContext.Orders.AddRangeAsync(new[] { order1, order2, order3 });
        await _dbContext.SaveChangesAsync();
    }
}
