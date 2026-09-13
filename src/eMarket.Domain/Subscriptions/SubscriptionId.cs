using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Subscriptions;

public sealed record SubscriptionId(Guid Value) : StronglyTypedId(Value)
{
    public static SubscriptionId New() => new(Guid.NewGuid());
    public static SubscriptionId Create(Guid value) => new(value);
}
