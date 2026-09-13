using FluentValidation;

namespace eMarket.Application.Sales.Carts.Commands.RemoveFromCart;

public sealed class RemoveFromCartCommandValidator
    : AbstractValidator<RemoveFromCartCommand>
{
    public RemoveFromCartCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();
    }
}
