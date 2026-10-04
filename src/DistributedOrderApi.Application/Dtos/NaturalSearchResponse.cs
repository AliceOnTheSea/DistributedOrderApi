namespace DistributedOrderApi.Application.Dtos;

public record NaturalSearchResponse(
    string Explanation,
    OrderSearchFilter Filter,
    IReadOnlyList<OrderDto> Orders
);
