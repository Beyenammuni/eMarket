using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Catalog.Products.Rules;

public sealed class ProductMustHaveStockRule
    : IBusinessRule
{
    private readonly int _stock;
    private readonly int _requested;

    public ProductMustHaveStockRule(
        int stock,
        int requested)
    {
        _stock = stock;
        _requested = requested;
    }

    public string Message =>
        "Not enough stock.";

    public bool IsBroken()
        => _stock < _requested;
}

