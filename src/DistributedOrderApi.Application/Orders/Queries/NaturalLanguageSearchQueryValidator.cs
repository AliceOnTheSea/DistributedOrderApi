using FluentValidation;

namespace DistributedOrderApi.Application.Orders.Queries;

public class NaturalLanguageSearchQueryValidator : AbstractValidator<NaturalLanguageSearchQuery>
{
    public NaturalLanguageSearchQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query must not be empty.")
            .MaximumLength(500).WithMessage("Query must not exceed 500 characters.");
    }
}
