using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Application.Dtos;
using MediatR;

namespace DistributedOrderApi.Application.Orders.Queries;

public class NaturalLanguageSearchQueryHandler : IRequestHandler<NaturalLanguageSearchQuery, NaturalSearchResponse>
{
    private readonly ILlmSearchFilterService _llmService;
    private readonly IOrderReadRepository _repository;

    public NaturalLanguageSearchQueryHandler(ILlmSearchFilterService llmService, IOrderReadRepository repository)
    {
        _llmService = llmService;
        _repository = repository;
    }

    public async Task<NaturalSearchResponse> Handle(NaturalLanguageSearchQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException("Search query cannot be empty.", nameof(request.Query));
        }

        if (request.Query.Length > 500)
        {
            throw new ArgumentException("Search query exceeds maximum allowed length of 500 characters.", nameof(request.Query));
        }

        var (filter, explanation) = await _llmService.TranslateQueryAsync(request.Query, cancellationToken);
        var orders = await _repository.SearchOrdersAsync(filter, cancellationToken);

        return new NaturalSearchResponse(explanation, filter, orders);
    }
}
