using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Businesses.Rules;

public sealed class BusinessMustHaveOwnerRule : IBusinessRule
{
    private readonly IEnumerable<BusinessMember> _members;

    public BusinessMustHaveOwnerRule(IEnumerable<BusinessMember> members)
    {
        _members = members;
    }

    public string Message =>
        "A business must have at least one owner.";

    public bool IsBroken()
    {
        return !_members.Any(x => x.Role == BusinessRole.Owner);
    }
}
