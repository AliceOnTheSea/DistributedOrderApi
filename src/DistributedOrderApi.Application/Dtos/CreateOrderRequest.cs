namespace DistributedOrderApi.Application.Dtos;

public record CreateOrderItemRequest(string ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record CreateOrderRequest(
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    AddressDto ShippingAddress,
    string Currency,
    List<CreateOrderItemRequest> Items);
