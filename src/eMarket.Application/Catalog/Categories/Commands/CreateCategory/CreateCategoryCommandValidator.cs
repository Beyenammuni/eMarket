using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Categories.Commands.CreateCategory
{
    public sealed class UpdateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>

    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
        }
    }
}
