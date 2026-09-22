namespace DistributedOrderApi.Application.Dtos;

public record OrderItemDto(
    Guid Id,
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal Subtotal);
