using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error InvalidAmount =
        new(
            "Payment.InvalidAmount",
            "Payment amount must be greater than zero.");
    public static readonly Error AlreadyExists =
    new(
        "Payment.AlreadyExists",
        "A payment already exists for this order.");

    public static readonly Error AlreadySucceeded =
        new(
            "Payment.AlreadySucceeded",
            "Payment has already succeeded.");

    public static readonly Error AlreadyFailed =
        new(
            "Payment.AlreadyFailed",
            "Payment has already failed.");

    public static readonly Error CannotRefund =
        new(
            "Payment.CannotRefund",
            "Payment cannot be refunded.");

    public static readonly Error NotFound =
        new(
            "Payment.NotFound",
            "Payment was not found.");
}
