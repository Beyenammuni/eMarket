using eMarket.Application.Catalog.Categories.Commands.DeactivateCategory;
using FluentValidation;

namespace eMarket.Application.Catalog.Categories.Commands.ActivateCategory;

public sealed class ActivateCategoryValidator
    : AbstractValidator<DeactivateCategoryCommand>
{
    public ActivateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id Of Categiry should not be Empty");
    }
}
