using FluentValidation;

namespace eMarket.Application.Catalog.Products.Commands.RemoveStock;

public sealed class RemoveStockCommandValidator
    : AbstractValidator<RemoveStockCommand>
{
    public RemoveStockCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
