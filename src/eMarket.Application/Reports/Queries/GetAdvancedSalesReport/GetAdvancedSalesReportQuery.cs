
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Reports.Queries.GetAdvancedSalesReport;

public sealed record GetAdvancedSalesReportQuery(
    Guid BusinessId,
    DateTime FromUtc,
    DateTime ToUtc
) : IRequest<Result<AdvancedSalesReportResponse>>;
