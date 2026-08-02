using FluentValidation;

namespace eMarket.Application.Catalog.Categories.Commands.DeactivateCategory;

public sealed class DeactivateCategoryValidator
    : AbstractValidator<DeactivateCategoryCommand>
{
    public DeactivateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id Of Categiry should not be Empty");
    }
}
