using FluentValidation;

namespace eMarket.Application.Sales.Orders.Commands.StartProcessing;

public sealed class StartProcessingCommandValidator
    : AbstractValidator<StartProcessingCommand>
{
    public StartProcessingCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
