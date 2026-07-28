using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Catalog.Products.Rules;

public sealed class ProductCannotHaveSamePriceRule
    : IBusinessRule
{
    private readonly Money _current;
    private readonly Money _new;

    public ProductCannotHaveSamePriceRule(
        Money current,
        Money @new)
    {
        _current = current;
        _new = @new;
    }

    public string Message =>
        "Product already has this price.";

    public bool IsBroken()
        => _current == _new;
}
