using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Sales.Carts;

public static class CartErrors
{
    public static readonly Error AlreadyCheckedOut =
        new(
            "Cart.AlreadyCheckedOut",
            "Cart has already been checked out.");
    public static readonly Error InsufficientStock =
    new(
        "Cart.Stock.Insufficient",
        "The requested quantity exceeds available stock.");
    public static readonly Error CartNotActive =
    new(
        "Cart.NotActive",
        "Cart is not active.");
    public static readonly Error Unauthorized =
        new(
            "Cart.Unauthorized",
            "Cart is Unauthorized.");
    public static readonly Error UserRequired =
        new(
            "Cart.UserRequired",
            "User is required.");

    public static readonly Error EmptyCart =
        new(
            "Cart.Empty",
            "Cart is empty.");

    public static readonly Error ProductAlreadyExists =
        new(
            "Cart.Product.Exists",
            "Product already exists in cart.");

    public static readonly Error ProductNotFound =
        new(
            "Cart.Product.NotFound",
            "Product was not found in cart.");

    public static readonly Error InvalidQuantity =
        new(
            "Cart.Quantity.Invalid",
            "Quantity must be greater than zero.");
}
