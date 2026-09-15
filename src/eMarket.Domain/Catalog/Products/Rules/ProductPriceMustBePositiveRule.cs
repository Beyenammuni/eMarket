using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Catalog.Products.Rules;

public sealed class ProductPriceMustBePositiveRule
    : IBusinessRule
{
    private readonly Money _money;

    public ProductPriceMustBePositiveRule(
        Money money)
    {
        _money = money;
    }

    public string Message =>
        "Product price must be greater than zero.";

    public bool IsBroken()
        => _money.Amount <= 0;
}
