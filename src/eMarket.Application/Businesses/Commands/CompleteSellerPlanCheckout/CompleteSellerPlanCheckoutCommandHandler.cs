
using System.Globalization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Commands.CompleteSellerPlanCheckout;

public sealed class CompleteSellerPlanCheckoutCommandHandler
    : IRequestHandler<CompleteSellerPlanCheckoutCommand, Result>
{
    private readonly IBusinessDbContext _businessDb;
    private readonly ISellerPlanPaymentGateway _paymentGateway;

    public CompleteSellerPlanCheckoutCommandHandler(
        IBusinessDbContext businessDb,
        ISellerPlanPaymentGateway paymentGateway)
    {
        _businessDb = businessDb;
        _paymentGateway = paymentGateway;
    }

    public async Task<Result> Handle(
        CompleteSellerPlanCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PaymentId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Token))
        {
            return Failure("Invalid callback data.");
        }

        // Read initial payment without tracking.
        var initialPayment = await _businessDb.SellerPlanPayments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.PaymentId,
                cancellationToken);

        if (initialPayment is null)
            return Failure("Seller plan payment was not found.");

        if (!string.Equals(
                initialPayment.ProviderToken,
                request.Token,
                StringComparison.Ordinal))
        {
            return Failure("Checkout token does not match.");
        }

        // Handle callbacks that were already completed.
        if (initialPayment.Status == "Paid")
        {
            var activeSubscription =
                await _businessDb.SellerPlanSubscriptions
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == initialPayment.SellerPlanSubscriptionId &&
                             x.Status == "Active",
                        cancellationToken);

            return activeSubscription
                ? Result.Success()
                : Failure("Payment has already been processed.");
        }

        if (initialPayment.Status != "Pending")
            return Failure("Payment is not pending.");

        // Verify payment with the provider before opening the transaction.
        var checkout = await _paymentGateway.GetCheckoutResultAsync(
            request.Token,
            cancellationToken);

        if (!checkout.IsSuccess ||
            string.IsNullOrWhiteSpace(checkout.PaymentId))
        {
            return Failure(
                checkout.Error ?? "Checkout verification failed.");
        }

        var expectedId = request.PaymentId.ToString();

        if (!string.Equals(
                checkout.ConversationId,
                expectedId,
                StringComparison.Ordinal) ||
            !string.Equals(
                checkout.BasketId,
                expectedId,
                StringComparison.Ordinal))
        {
            return Failure("Checkout identifiers do not match.");
        }

        if (!decimal.TryParse(
                checkout.Price,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var checkoutPrice) ||
            !decimal.TryParse(
                checkout.PaidPrice,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var paidPrice))
        {
            return Failure("Invalid checkout amount.");
        }

        Result operationResult = Failure("Payment processing failed.");

        await _businessDb.ExecuteInSerializableTransactionAsync(
            async ct =>
            {
                // Re-read payment inside the transaction.
                var payment = await _businessDb.SellerPlanPayments
                    .FirstOrDefaultAsync(
                        x => x.Id == request.PaymentId,
                        ct);

                if (payment is null)
                {
                    operationResult =
                        Failure("Seller plan payment was not found.");
                    return;
                }

                // Re-check token and status to prevent concurrent callbacks.
                if (!string.Equals(
                        payment.ProviderToken,
                        request.Token,
                        StringComparison.Ordinal))
                {
                    operationResult =
                        Failure("Checkout token does not match.");
                    return;
                }

                var subscription =
                    await _businessDb.SellerPlanSubscriptions
                        .FirstOrDefaultAsync(
                            x => x.Id == payment.SellerPlanSubscriptionId,
                            ct);

                if (subscription is null)
                {
                    operationResult =
                        Failure("Seller plan subscription was not found.");
                    return;
                }

                if (payment.Status == "Paid")
                {
                    operationResult =
                        subscription.Status == "Active"
                            ? Result.Success()
                            : Failure("Payment has already been processed.");

                    return;
                }

                if (payment.Status != "Pending" ||
                    subscription.Status != "Pending")
                {
                    operationResult = Failure("Payment is not pending.");
                    return;
                }

                if (checkoutPrice != payment.Amount ||
                    paidPrice != payment.Amount)
                {
                    operationResult =
                        Failure("Paid amount does not match the seller plan price.");
                    return;
                }

                if (!string.Equals(
                        checkout.Currency,
                        payment.Currency,
                        StringComparison.OrdinalIgnoreCase))
                {
                    operationResult =
                        Failure("Payment currency does not match.");
                    return;
                }

                var plan = await _businessDb.SellerPlans
                    .FirstOrDefaultAsync(
                        x => x.Id == payment.SellerPlanId,
                        ct);

                if (plan is null || plan.DurationDays <= 0)
                {
                    operationResult =
                        Failure("Seller plan is unavailable.");
                    return;
                }

                var now = DateTime.UtcNow;

                var currentSubscriptionExpiry =
                    await _businessDb.SellerPlanSubscriptions
                        .Where(x =>
                            x.BusinessId == payment.BusinessId &&
                            x.Status == "Active" &&
                            x.ExpiresAt > now)
                        .Select(x => (DateTime?)x.ExpiresAt)
                        .MaxAsync(ct);

                var activationStart =
                    currentSubscriptionExpiry.HasValue &&
                    currentSubscriptionExpiry.Value > now
                        ? currentSubscriptionExpiry.Value
                        : now;

                payment.MarkAsPaid(checkout.PaymentId);

                subscription.Activate(
                    activationStart,
                    plan.DurationDays,
                    checkout.PaymentId);

                await _businessDb.SaveChangesAsync(ct);

                operationResult = Result.Success();
            },
            cancellationToken);

        return operationResult;
    }

    private static Result Failure(string message)
    {
        return Result.Failure(
            new Error(
                "SellerPlanCheckout.VerificationFailed",
                message));
    }
}
