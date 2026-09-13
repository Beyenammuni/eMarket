using eMarket.SharedKernel.Results;
using MediatR;
using eMarket.Application.Subscriptions.Commands.CreateSubscription;

namespace eMarket.Application.Subscriptions.Commands.UpdateSubscription;
public sealed record UpdateSubscriptionCommand(Guid SubscriptionId, DayOfWeek DeliveryDay, IReadOnlyCollection<SubscriptionItemRequest> Items) : IRequest<Result<UpdateSubscriptionResponse>>;
public sealed record UpdateSubscriptionResponse(Guid Id, DayOfWeek DeliveryDay, DateTime NextDeliveryDate, string Status);
