
using FluentValidation;

namespace eMarket.Application.Catalog.Products.Commands.AddProductOption;

public sealed class AddProductOptionValidator
    : AbstractValidator<AddProductOptionCommand>
{
    public AddProductOptionValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required.");

        RuleFor(x => x.OptionGroupId)
            .NotEmpty()
            .WithMessage("Option group ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Option name is required.")
            .MaximumLength(100)
            .WithMessage("Option name cannot exceed 100 characters.");

        RuleFor(x => x.AdditionalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Additional price cannot be negative.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order cannot be negative.");
    }
}
