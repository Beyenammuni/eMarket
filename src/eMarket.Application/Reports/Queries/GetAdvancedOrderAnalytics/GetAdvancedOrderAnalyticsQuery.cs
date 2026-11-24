
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Reports.Queries.GetAdvancedOrderAnalytics;

public sealed record GetAdvancedOrderAnalyticsQuery(
    Guid BusinessId,
    DateTime FromUtc,
    DateTime ToUtc
) : IRequest<Result<AdvancedOrderAnalyticsResponse>>;
