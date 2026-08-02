using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Businesses.Entities;

public sealed record BusinessMemberId(Guid Value)
    : StronglyTypedId(Value)
{
    public static BusinessMemberId New()
        => new(Guid.NewGuid());

    public static BusinessMemberId Create(Guid value)
        => new(value);
}
