using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Common;
using RulesIBR = eMarket.SharedKernel.Rules.IBusinessRule;

namespace eMarket.Domain.Businesses.Rules;

public sealed class OnlyOwnerCanTransferOwnershipRule : RulesIBR
{
    private readonly BusinessRole _role;

    public OnlyOwnerCanTransferOwnershipRule(
        BusinessRole role)
    {
        _role = role;
    }

    public string Message =>
        "Only the owner can transfer ownership.";

    public bool IsBroken()
    {
        return _role != BusinessRole.Owner;
    }
}
