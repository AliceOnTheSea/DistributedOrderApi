using DistributedOrderApi.Application.Orders.Commands;
using FluentValidation;

namespace DistributedOrderApi.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.")
            .MaximumLength(50).WithMessage("Customer ID cannot exceed 50 characters.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer Name is required.")
            .MaximumLength(100).WithMessage("Customer Name cannot exceed 100 characters.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Customer Email is required.")
            .EmailAddress().WithMessage("A valid email address must be specified.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping Address is required.");

        RuleFor(x => x.ShippingAddress.Street)
            .NotEmpty().WithMessage("Street address is required.")
            .When(x => x.ShippingAddress != null);

        RuleFor(x => x.ShippingAddress.City)
            .NotEmpty().WithMessage("City is required.")
            .When(x => x.ShippingAddress != null);

        RuleFor(x => x.ShippingAddress.State)
            .NotEmpty().WithMessage("State is required.")
            .When(x => x.ShippingAddress != null);

        RuleFor(x => x.ShippingAddress.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .When(x => x.ShippingAddress != null);

        RuleFor(x => x.ShippingAddress.Country)
            .NotEmpty().WithMessage("Country is required.")
            .When(x => x.ShippingAddress != null);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one line item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("Product ID is required.");
            item.RuleFor(i => i.ProductName).NotEmpty().WithMessage("Product Name is required.");
            item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("Unit price must be greater than zero.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
