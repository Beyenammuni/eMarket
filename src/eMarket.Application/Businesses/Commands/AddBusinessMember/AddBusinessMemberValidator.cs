using FluentValidation;

namespace eMarket.Application.Businesses.Commands.AddBusinessMember;

public sealed class AddBusinessMemberValidator
    : AbstractValidator<AddBusinessMemberCommand>
{
    public AddBusinessMemberValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Role)
            .InclusiveBetween(1, 4);
    }
}
