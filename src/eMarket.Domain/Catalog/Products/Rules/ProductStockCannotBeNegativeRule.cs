using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Catalog.Products.Rules;

public sealed class ProductStockCannotBeNegativeRule
    : IBusinessRule
{
    private readonly int _quantity;

    public ProductStockCannotBeNegativeRule(
        int quantity)
    {
        _quantity = quantity;
    }

    public string Message =>
        "Stock cannot be negative.";

    public bool IsBroken()
        => _quantity < 0;
}
