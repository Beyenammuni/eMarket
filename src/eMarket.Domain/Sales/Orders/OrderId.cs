using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Sales.Orders;

public sealed record OrderId(Guid Value)
    : StronglyTypedId(Value)
{
    public static OrderId New()
        => new(Guid.NewGuid());

    public static OrderId Create(Guid value)
        => new(value);
}
