using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Businesses.Rules;

public sealed class MemberCannotBeAddedTwiceRule : IBusinessRule
{
    private readonly IEnumerable<BusinessMember> _members;
    private readonly UserId _userId;

    public MemberCannotBeAddedTwiceRule(
        IEnumerable<BusinessMember> members,
        UserId userId)
    {
        _members = members;
        _userId = userId;
    }

    public string Message =>
        "The user is already a member of this business.";

    public bool IsBroken()
    {
        return _members.Any(x =>
            x.UserId == _userId &&
            x.IsActive);
    }
}
