using DistributedOrderApi.Application.Dtos;

namespace DistributedOrderApi.Application.Common.Interfaces;

public interface IOrderReadRepository
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
    Task<OrderSummaryDto> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
}
