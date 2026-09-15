using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Catalog.Products.Rules;

public sealed class ProductCannotHaveSameNameRule
    : IBusinessRule
{
    private readonly ProductName _current;
    private readonly ProductName _new;

    public ProductCannotHaveSameNameRule(
        ProductName current,
        ProductName @new)
    {
        _current = current;
        _new = @new;
    }

    public string Message =>
        "Product already has this name.";

    public bool IsBroken()
        => _current == _new;
}
