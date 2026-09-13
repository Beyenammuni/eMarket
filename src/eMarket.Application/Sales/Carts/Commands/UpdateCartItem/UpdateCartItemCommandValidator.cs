using FluentValidation;

namespace eMarket.Application.Sales.Carts.Commands.UpdateCartItem;

public sealed class UpdateCartItemCommandValidator
    : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
