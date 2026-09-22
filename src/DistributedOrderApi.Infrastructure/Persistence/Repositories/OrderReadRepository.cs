using System.Data;
using Dapper;
using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DistributedOrderApi.Infrastructure.Persistence.Repositories;

public class OrderReadRepository : IOrderReadRepository
{
    private readonly string _connectionString;

    public OrderReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                o.Id, o.CustomerId, o.CustomerName, o.CustomerEmail,
                o.ShippingStreet AS Street, o.ShippingCity AS City, o.ShippingState AS State, 
                o.ShippingZipCode AS ZipCode, o.ShippingCountry AS Country,
                o.Status, o.PaymentStatus, o.TotalAmount, o.Currency,
                o.CreatedAtUtc, o.UpdatedAtUtc,
                i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Currency, i.Quantity
            FROM Orders o
            LEFT JOIN OrderItems i ON o.Id = i.OrderId
            WHERE o.Id = @Id";

        using var connection = CreateConnection();
        var orderDictionary = new Dictionary<Guid, OrderDto>();

        await connection.QueryAsync<OrderDto, AddressDto, OrderItemDto, OrderDto>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken),
            (order, address, item) =>
            {
                if (!orderDictionary.TryGetValue(order.Id, out var currentOrder))
                {
                    currentOrder = order with { ShippingAddress = address, Items = new List<OrderItemDto>() };
                    orderDictionary.Add(currentOrder.Id, currentOrder);
                }

                if (item != null && item.Id != Guid.Empty)
                {
                    var itemWithSubtotal = item with { Subtotal = item.UnitPrice * item.Quantity };
                    currentOrder.Items.Add(itemWithSubtotal);
                }

                return currentOrder;
            },
            splitOn: "Street,Id");

        return orderDictionary.Values.FirstOrDefault();
    }

    public async Task<IReadOnlyList<OrderDto>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                o.Id, o.CustomerId, o.CustomerName, o.CustomerEmail,
                o.ShippingStreet AS Street, o.ShippingCity AS City, o.ShippingState AS State, 
                o.ShippingZipCode AS ZipCode, o.ShippingCountry AS Country,
                o.Status, o.PaymentStatus, o.TotalAmount, o.Currency,
                o.CreatedAtUtc, o.UpdatedAtUtc,
                i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Currency, i.Quantity
            FROM Orders o
            LEFT JOIN OrderItems i ON o.Id = i.OrderId
            WHERE o.CustomerId = @CustomerId
            ORDER BY o.CreatedAtUtc DESC";

        using var connection = CreateConnection();
        var orderDictionary = new Dictionary<Guid, OrderDto>();

        await connection.QueryAsync<OrderDto, AddressDto, OrderItemDto, OrderDto>(
            new CommandDefinition(sql, new { CustomerId = customerId }, cancellationToken: cancellationToken),
            (order, address, item) =>
            {
                if (!orderDictionary.TryGetValue(order.Id, out var currentOrder))
                {
                    currentOrder = order with { ShippingAddress = address, Items = new List<OrderItemDto>() };
                    orderDictionary.Add(currentOrder.Id, currentOrder);
                }

                if (item != null && item.Id != Guid.Empty)
                {
                    var itemWithSubtotal = item with { Subtotal = item.UnitPrice * item.Quantity };
                    currentOrder.Items.Add(itemWithSubtotal);
                }

                return currentOrder;
            },
            splitOn: "Street,Id");

        return orderDictionary.Values.ToList();
    }

    public async Task<OrderSummaryDto> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                COUNT(*) AS TotalOrders,
                SUM(CASE WHEN Status IN ('Draft', 'Submitted', 'Processing') THEN 1 ELSE 0 END) AS PendingOrders,
                SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedOrders,
                SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledOrders,
                COALESCE(SUM(CASE WHEN Status = 'Completed' THEN TotalAmount ELSE 0 END), 0) AS TotalRevenueUsd
            FROM Orders";

        using var connection = CreateConnection();
        var summary = await connection.QueryFirstOrDefaultAsync<OrderSummaryDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return summary ?? new OrderSummaryDto(0, 0, 0, 0, 0);
    }
}
