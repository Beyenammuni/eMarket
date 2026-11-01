
using MediatR;

namespace eMarket.Application.Businesses.Commands.StartSellerPlanSubscription;

public sealed record StartSellerPlanSubscriptionCommand(
    Guid BusinessId,
    Guid SellerPlanId,
    string ReturnUrl
) : IRequest<StartSellerPlanSubscriptionResult>;

public sealed record StartSellerPlanSubscriptionResult(
    Guid SubscriptionId,
    Guid PaymentId,
    string PaymentUrl);
