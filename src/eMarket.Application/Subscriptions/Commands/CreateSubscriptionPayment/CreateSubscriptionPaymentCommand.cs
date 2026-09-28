using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Subscriptions.Commands.CreateSubscriptionPayment;

public sealed record CreateSubscriptionPaymentCommand(
    Guid SubscriptionId,
    string ReturnUrl)
    : IRequest<Result<CreateSubscriptionPaymentResponse>>;

public sealed record CreateSubscriptionPaymentResponse(
    Guid PaymentId,
    Guid SubscriptionId,
    decimal Amount,
    string Currency,
    string PaymentUrl);

