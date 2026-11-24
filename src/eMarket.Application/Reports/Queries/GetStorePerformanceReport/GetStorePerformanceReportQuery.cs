
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Reports.Queries.GetStorePerformanceReport;

public sealed record GetStorePerformanceReportQuery(
    Guid BusinessId,
    DateTime FromUtc,
    DateTime ToUtc
) : IRequest<Result<StorePerformanceReportResponse>>;
