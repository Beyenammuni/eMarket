using eMarket.SharedKernel.Results;
using MediatR;
namespace eMarket.Application.Subscriptions.Commands.SkipSubscription;
public sealed record SkipSubscriptionCommand(Guid SubscriptionId) : IRequest<Result<SkipSubscriptionResponse>>;
public sealed record SkipSubscriptionResponse(Guid Id, DateTime NextDeliveryDate);
