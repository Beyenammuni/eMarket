using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Businesses.Rules;

public sealed class CannotRemoveLastOwnerRule : IBusinessRule
{
    private readonly IEnumerable<BusinessMember> _members;
    private readonly UserId _userId;

    public CannotRemoveLastOwnerRule(
        IEnumerable<BusinessMember> members,
        UserId userId)
    {
        _members = members;
        _userId = userId;
    }

    public string Message =>
        "The last owner of the business cannot be removed.";

    public bool IsBroken()
    {
        var member = _members.FirstOrDefault(x =>
            x.UserId == _userId &&
            x.IsActive);

        if (member is null)
        {
            return false;
        }

        if (member.Role != BusinessRole.Owner)
        {
            return false;
        }

        var ownerCount = _members.Count(x =>
            x.Role == BusinessRole.Owner &&
            x.IsActive);

        return ownerCount == 1;
    }
}
