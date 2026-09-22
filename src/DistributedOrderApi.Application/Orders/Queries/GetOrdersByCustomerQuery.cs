using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using MediatR;

namespace DistributedOrderApi.Application.Orders.Queries;

public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<IReadOnlyList<OrderDto>>;

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderReadRepository _readRepository;

    public GetOrdersByCustomerQueryHandler(IOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
    }
}
