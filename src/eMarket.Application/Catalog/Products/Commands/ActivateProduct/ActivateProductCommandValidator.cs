using FluentValidation;

namespace eMarket.Application.Catalog.Products.Commands.ActivateProduct;

public sealed class ActivateProductCommandValidator
    : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
