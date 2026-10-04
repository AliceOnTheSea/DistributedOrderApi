namespace DistributedOrderApi.Application.Dtos;

public record OrderSearchFilter(
    string? Status = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    string? CustomerId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? SearchKeyword = null
);
