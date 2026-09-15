using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Catalog.Products;

public sealed record ProductId(Guid Value)
    : StronglyTypedId(Value)
{
    public static ProductId New()
        => new(Guid.NewGuid());

    public static ProductId Create(Guid value)
        => new(value);
}
