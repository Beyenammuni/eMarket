using FluentValidation;

namespace eMarket.Application.Catalog.Products.Commands.DeactivateProduct;

public sealed class DeactivateProductCommandValidator
    : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
