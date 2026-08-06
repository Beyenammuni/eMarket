using FluentValidation;

namespace eMarket.Application.Businesses.Commands.ChangeMemberRole;

public sealed class ChangeMemberRoleValidator
    : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Role)
            .InclusiveBetween(1, 4);
    }
}
