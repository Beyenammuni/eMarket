using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Subscriptions.Commands.CompleteSubscriptionCheckout;

public sealed record CompleteSubscriptionCheckoutCommand(
    Guid PaymentId,
    string Token)
    : IRequest<Result>;
