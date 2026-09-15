using FluentValidation;

namespace eMarket.Application.Sales.Orders.Commands.Cancel;

public sealed class CancelOrderCommandValidator
    : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
