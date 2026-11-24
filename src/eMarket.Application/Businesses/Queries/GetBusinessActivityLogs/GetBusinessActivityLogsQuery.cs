
using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Queries.GetBusinessActivityLogs;

public sealed record GetBusinessActivityLogsQuery(
    Guid BusinessId,
    int Page = 1,
    int PageSize = 20)
    : IRequest<Result<GetBusinessActivityLogsResponse>>;
