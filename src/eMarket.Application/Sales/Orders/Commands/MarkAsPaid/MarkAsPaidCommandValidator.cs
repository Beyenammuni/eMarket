using FluentValidation;

namespace eMarket.Application.Sales.Orders.Commands.MarkAsPaid;

public sealed class MarkAsPaidCommandValidator
    : AbstractValidator<MarkAsPaidCommand>
{
    public MarkAsPaidCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
