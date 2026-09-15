using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Catalog.Products;

public static class ProductErrors
{
    public static readonly Error SameName =
        new(
            "Product.Name.Same",
            "Product already has this name.");

    public static readonly Error Forbidden = new(
    "Product.Forbidden",
    "You do not have permission to perform this action.");
    public static readonly Error AlreadyDeleted =
    new(
        "Product.AlreadyDeleted",
        "Product is already deleted.");
    public static readonly Error BusinessRequired =
    new(
        "Product.BusinessRequired",
        "Business is Required");
    public static readonly Error ProductNotAvailable =
    new(
        "Product.NotAvailable",
        "Product is not available.");

    public static readonly Error InsufficientStock =
        new(
            "Product.Stock.Insufficient",
            "There is not enough stock available.");


    public static readonly Error AlreadyExists =
        new(
            "Product.AlreadyExists",
            "A product with the same name already exists in this business.");

    public static readonly Error NotFound =
        new(
            "Product.NotFound",
            "Product was not found.");

    public static readonly Error CategoryNotFound =
        new(
            "Product.Category.NotFound",
            "Category was not found.");

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
            "Stock quantity is invalid.");
    public static readonly Error BusinessAccessDenied = new(
    "Product.BusinessAccessDenied",
    "You do not have access to this business.");
    public static readonly Error Unauthorized = new(
    "Product.Unauthorized",
    "You must be authenticated.");
}
