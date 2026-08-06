using System.Linq;
using eMarket.Domain.Businesses.ValueObjects;
using FluentValidation;

namespace eMarket.Application.Businesses.Commands.CreateBusiness;

public sealed class CreateBusinessCommandValidator
    : AbstractValidator<CreateBusinessCommand>
{
    public CreateBusinessCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Business name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .NotNull()
            .WithMessage("Business type is required.")
            ;
    }
}
