using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

    namespace eMarket.Application.Businesses.Commands.UpdateBusiness;

public sealed class UpdateBusinessValidator
    : AbstractValidator<UpdateBusinessCommand>
{
    public UpdateBusinessValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .InclusiveBetween(1, 5);
    }
}
