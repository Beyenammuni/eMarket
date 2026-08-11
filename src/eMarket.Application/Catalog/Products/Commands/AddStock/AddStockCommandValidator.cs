using FluentValidation;

namespace eMarket.Application.Catalog.Products.Commands.AddStock;

public sealed class AddStockCommandValidator
    : AbstractValidator<AddStockCommand>
{
    public AddStockCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
