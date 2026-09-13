using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Subscriptions.Commands.GenerateSubscriptionOrder;

public sealed record GenerateSubscriptionOrderCommand(
    Guid SubscriptionId)
    : IRequest<Result<GenerateSubscriptionOrderResponse>>;

public sealed record GenerateSubscriptionOrderResponse(
    Guid SubscriptionId,
    Guid OrderId,
    DateTime NextDeliveryDate);
