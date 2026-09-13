using eMarket.SharedKernel.Results;
using MediatR;
namespace eMarket.Application.Subscriptions.Queries.GetMySubscription;
public sealed record GetMySubscriptionsQuery : IRequest<Result<IReadOnlyCollection<GetMySubscriptionResponse>>>;
