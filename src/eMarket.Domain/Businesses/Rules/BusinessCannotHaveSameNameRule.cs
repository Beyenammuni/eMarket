using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Rules;

using IBusinessRuleAlias = eMarket.SharedKernel.Common.IBusinessRule;

namespace eMarket.Domain.Businesses.Rules;

public sealed class BusinessCannotHaveSameNameRule : IBusinessRuleAlias
{
    private readonly BusinessName _current;
    private readonly BusinessName _newName;

    public BusinessCannotHaveSameNameRule(
        BusinessName current,
        BusinessName newName)
    {
        _current = current;
        _newName = newName;
    }

    public string Message =>
        "Business already has this name.";

    public bool IsBroken()
    {
        return _current == _newName;
    }
}
