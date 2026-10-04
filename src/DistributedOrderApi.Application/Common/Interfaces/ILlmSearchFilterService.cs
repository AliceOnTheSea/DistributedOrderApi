using DistributedOrderApi.Application.Dtos;

namespace DistributedOrderApi.Application.Common.Interfaces;

public interface ILlmSearchFilterService
{
    Task<(OrderSearchFilter Filter, string Explanation)> TranslateQueryAsync(string naturalQuery, CancellationToken cancellationToken = default);
}
