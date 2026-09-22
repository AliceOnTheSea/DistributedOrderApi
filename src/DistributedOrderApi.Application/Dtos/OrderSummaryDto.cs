namespace DistributedOrderApi.Application.Dtos;

public record OrderSummaryDto(
    int TotalOrders,
    int PendingOrders,
    int CompletedOrders,
    int CancelledOrders,
    decimal TotalRevenueUsd);
