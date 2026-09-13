using FluentValidation;

namespace eMarket.Application.Sales.Carts.Commands.AddToCart;

public sealed class AddToCartCommandValidator
    : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
