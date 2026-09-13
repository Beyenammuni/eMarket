using FluentValidation;

namespace eMarket.Application.Sales.Orders.Commands.Ship;

public sealed class ShipCommandValidator
    : AbstractValidator<ShipCommand>
{
    public ShipCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
