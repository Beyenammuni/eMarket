using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Domain.Payments;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Subscriptions.Commands.CompleteSubscriptionCheckout;

internal sealed class CompleteSubscriptionCheckoutCommandHandler
    : IRequestHandler<CompleteSubscriptionCheckoutCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionDbContext _subscriptionContext;
    private readonly ISubscriptionPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteSubscriptionCheckoutCommandHandler(
        IPaymentRepository paymentRepository,
        ISubscriptionDbContext subscriptionContext,
        ISubscriptionPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _subscriptionContext = subscriptionContext;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CompleteSubscriptionCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(
            PaymentId.Create(request.PaymentId),
            cancellationToken);

        if (payment is null)
        {
            return Result.Failure(
                PaymentErrors.NotFound);
        }

        if (payment.SubscriptionId is null)
        {
            return Result.Failure(
                SubscriptionErrors.NotFound);
        }

        var subscription =
            await _subscriptionContext.Subscriptions
                .FirstOrDefaultAsync(
                    x => x.Id == SubscriptionId.Create(
                        payment.SubscriptionId.Value),
                    cancellationToken);

        if (subscription is null)
        {
            return Result.Failure(
                SubscriptionErrors.NotFound);
        }

        var checkoutResult =
            await _paymentGateway.GetCheckoutResultAsync(
                request.Token,
                cancellationToken);

        if (!checkoutResult.IsSuccess ||
            string.IsNullOrWhiteSpace(
                checkoutResult.SubscriptionReferenceCode))
        {
            return Result.Failure(
                SubscriptionErrors.NotActive);
        }

        var referenceResult =
     subscription.SetIyzicoSubscriptionReference(
         checkoutResult.SubscriptionReferenceCode);

        if (referenceResult.IsFailure)
        {
            return referenceResult;
        }

        var paymentResult =
            payment.MarkAsSucceeded(
                "iyzico",
                checkoutResult.SubscriptionReferenceCode);

        if (paymentResult.IsFailure)
        {
            return paymentResult;
        }

        var activationResult =
            subscription.Activate();

        if (activationResult.IsFailure)
        {
            return activationResult;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}
