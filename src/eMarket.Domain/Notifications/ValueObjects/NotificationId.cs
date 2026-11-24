using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Notifications.ValueObjects;

public sealed record NotificationId(Guid Value)
    : StronglyTypedId(Value)
{
    public static NotificationId New()
        => new(Guid.NewGuid());

    public static NotificationId Create(Guid value)
        => new(value);
}
