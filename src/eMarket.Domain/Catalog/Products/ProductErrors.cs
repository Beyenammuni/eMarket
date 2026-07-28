using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Catalog.Products;

public static class ProductErrors
{
    public static readonly Error SameName =
        new(
            "Product.Name.Same",
            "Product already has this name.");
    public static readonly Error ProductPriceMustBePositive =
        new(
            "Product.Price.Positive",
            "Product price must be positive.");

    public static readonly Error SamePrice =
        new(
            "Product.Price.Same",
            "Product already has this price.");

    public static readonly Error AlreadyActive =
        new(
            "Product.Status.Active",
            "Product is already active.");

    public static readonly Error AlreadyInactive =
        new(
            "Product.Status.Inactive",
            "Product is already inactive.");

    public static readonly Error InvalidStock =
        new(
            "Product.Stock.Invalid",
            "Stock cannot be negative.");
}
