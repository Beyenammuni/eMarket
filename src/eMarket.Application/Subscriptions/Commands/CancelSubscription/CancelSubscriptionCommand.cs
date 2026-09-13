using eMarket.SharedKernel.Results;
using MediatR;
namespace eMarket.Application.Subscriptions.Commands.CancelSubscription;
public sealed record CancelSubscriptionCommand(Guid SubscriptionId) : IRequest<Result>;
