
using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Promotions.ValueObjects;

public sealed record PromotionId(Guid Value) : StronglyTypedId(Value)
{
    public static PromotionId New()
        => new(Guid.NewGuid());

    public static PromotionId Create(Guid value)
        => new(value);
}
