using FluentValidation;

namespace eMarket.Application.Businesses.Commands.RemoveMember;

public sealed class RemoveMemberValidator
    : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
