using DistributedOrderApi.Application.Dtos;
using MediatR;

namespace DistributedOrderApi.Application.Orders.Queries;

public record NaturalLanguageSearchQuery(string Query) : IRequest<NaturalSearchResponse>;
