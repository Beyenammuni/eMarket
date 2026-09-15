using FluentValidation;

namespace eMarket.Application.Sales.Orders.Commands.Deliver;

public sealed class DeliverCommandValidator
    : AbstractValidator<DeliverCommand>
{
    public DeliverCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
