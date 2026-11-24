using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Businesses;

public sealed record BranchId(Guid Value) : StronglyTypedId(Value)
{
    public static BranchId New()
        => new(Guid.NewGuid());

    public static BranchId Create(Guid value)
        => new(value);
}
