using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Payments;

public sealed record PaymentId(Guid Value)
    : StronglyTypedId(Value)
{
    public static PaymentId New()
        => new(Guid.NewGuid());

    public static PaymentId Create(Guid value)
        => new(value);
}
