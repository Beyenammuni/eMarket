using FluentValidation;

namespace eMarket.Application.Catalog.Products.Commands.CreateProduct;

public sealed class CreateProductValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);
    }
}
