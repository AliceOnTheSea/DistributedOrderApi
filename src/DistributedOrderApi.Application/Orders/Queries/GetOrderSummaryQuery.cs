using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using MediatR;

namespace DistributedOrderApi.Application.Orders.Queries;

public record GetOrderSummaryQuery : IRequest<OrderSummaryDto>;

public class GetOrderSummaryQueryHandler : IRequestHandler<GetOrderSummaryQuery, OrderSummaryDto>
{
    private readonly IOrderReadRepository _readRepository;

    public GetOrderSummaryQueryHandler(IOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<OrderSummaryDto> Handle(GetOrderSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetSummaryMetricsAsync(cancellationToken);
    }
}
