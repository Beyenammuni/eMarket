using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Catalog;

public sealed record CategoryId(Guid Value)
    : StronglyTypedId(Value)
{
    public static CategoryId New()
        => new(Guid.NewGuid());

    public static CategoryId Create(Guid value)
        => new(value);
}
