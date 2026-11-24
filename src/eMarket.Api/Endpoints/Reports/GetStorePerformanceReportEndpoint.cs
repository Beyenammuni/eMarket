
using eMarket.Api.Common;
using eMarket.Application.Reports.Queries.GetStorePerformanceReport;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Reports;

public static class GetStorePerformanceReportEndpoint
{
    public static IEndpointRouteBuilder MapGetStorePerformanceReportEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/seller/reports/store-performance",
            async (
                Guid? businessId,
                DateTime fromUtc,
                DateTime toUtc,
                ISender sender,
                ICurrentUser currentUser,
                IBusinessAuthorization authorization,
                CancellationToken cancellationToken) =>
            {
                if (businessId is null || businessId == Guid.Empty)
                    return Results.BadRequest(
                        new { message = "BusinessId is required." });

                if (currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                var isSuperAdmin =
                    currentUser.IsInRole(Roles.SuperAdmin);

                if (!isSuperAdmin)
                {
                    var allowed = await authorization.HasPermissionAsync(
                        BusinessId.Create(businessId.Value),
                        eMarket.Application.Common.Authorization.Permissions.Business.View,
                        cancellationToken);

                    if (!allowed)
                        return ApiResults.Forbidden();
                }

                var query = new GetStorePerformanceReportQuery(
                    businessId.Value,
                    fromUtc,
                    toUtc);

                var result = await sender.Send(
                    query,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetStorePerformanceReport")
            .WithTags("Seller Reports");

        return app;
    }
}
