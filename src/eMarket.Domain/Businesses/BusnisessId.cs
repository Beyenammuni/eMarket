using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Businesses;

public sealed record BusinessId(Guid Value) : StronglyTypedId(Value)
{
    public static BusinessId New()
        => new(Guid.NewGuid());

    public static BusinessId Create(Guid value)
        => new(value);
}
