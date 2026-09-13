using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Sales.Orders;

public static class OrderErrors
{
    public static readonly Error CartNotFound =
    new(
        "Order.CartNotFound",
        "Active cart was not found.");
    public static readonly Error NotFound =
    new(
        "Order.NotFound",
        "Order was not found.");

    public static readonly Error ProductNotFound =
        new(
            "Order.ProductNotFound",
            "Product was not found.");

    public static readonly Error ProductNotAvailable =
        new(
            "Order.ProductNotAvailable",
            "Product is not available.");

    public static readonly Error InsufficientStock =
        new(
            "Order.InsufficientStock",
            "Insufficient product stock.");
    public static readonly Error EmptyOrder =
        new(
            "Order.Empty",
            "Order cannot be empty.");

    public static readonly Error InvalidStatus =
        new(
            "Order.Status.Invalid",
            "The order status is invalid.");

    public static readonly Error CannotCancel =
        new(
            "Order.Cancel.Invalid",
            "The order cannot be cancelled.");

    public static readonly Error InvalidQuantity =
    new(
        "Order.Quantity.Invalid",
        "Order item quantity must be greater than zero.");

    public static readonly Error OrderNotFound =
        new(
            "Order.NotFound",
            "Order was not found.");
}
