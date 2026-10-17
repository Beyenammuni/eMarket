using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Payments;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Sales.Payments.Queries.GetSubscriptionPayment;

internal sealed class GetSubscriptionPaymentQueryHandler
    : IRequestHandler<
        GetSubscriptionPaymentQuery,
        Result<GetSubscriptionPaymentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionDbContext _subscriptionContext;
    private readonly ICurrentUser _currentUser;

    public GetSubscriptionPaymentQueryHandler(
        IPaymentRepository paymentRepository,
        ISubscriptionDbContext subscriptionContext,
        ICurrentUser currentUser)
    {
        _paymentRepository = paymentRepository;
        _subscriptionContext = subscriptionContext;
        _currentUser = currentUser;
    }

    public async Task<Result<GetSubscriptionPaymentResponse>> Handle(
        GetSubscriptionPaymentQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is null)
        {
            return Result<GetSubscriptionPaymentResponse>.Failure(
                new Error(
                    "Auth.Unauthorized",
                    "User is not authenticated."));
        }

        var subscription = await _subscriptionContext.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == SubscriptionId.Create(
                        request.SubscriptionId) &&
                    x.UserId == _currentUser.UserId,
                cancellationToken);

        if (subscription is null)
        {
            return Result<GetSubscriptionPaymentResponse>.Failure(
                SubscriptionErrors.NotFound);
        }

        var payment = await _paymentRepository
            .GetBySubscriptionIdAsync(
                request.SubscriptionId,
                cancellationToken);

        if (payment is null)
        {
            return Result<GetSubscriptionPaymentResponse>.Failure(
                PaymentErrors.NotFound);
        }

        return Result<GetSubscriptionPaymentResponse>.Success(
            new GetSubscriptionPaymentResponse(
                payment.Id.Value,
                request.SubscriptionId,
                payment.Amount.Amount,
                payment.Amount.Currency,
                payment.Status.ToString(),
                payment.Provider,
                payment.ProviderPaymentId,
                payment.CreatedAt));
    }
}
