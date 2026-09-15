using eMarket.SharedKernel.Results;
using MediatR;
namespace eMarket.Application.Subscriptions.Queries.GetMySubscription;
public sealed record GetMySubscriptionQuery(Guid SubscriptionId) : IRequest<Result<GetMySubscriptionResponse>>;
public sealed record GetMySubscriptionResponse(Guid Id,Guid BusinessId,DayOfWeek DeliveryDay,DateTime NextDeliveryDate,string Status,IReadOnlyCollection<SubscriptionItemResponse> Items);
public sealed record SubscriptionItemResponse(Guid ProductId,int Quantity);
