namespace DistributedOrderApi.Application.Dtos;

public record AddressDto(string Street, string City, string State, string ZipCode, string Country);

public record OrderDto(
    Guid Id,
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    AddressDto ShippingAddress,
    string Status,
    string PaymentStatus,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<OrderItemDto> Items);
