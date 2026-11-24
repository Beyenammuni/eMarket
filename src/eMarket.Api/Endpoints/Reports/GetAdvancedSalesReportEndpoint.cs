
using eMarket.Api.Common;
using eMarket.Application.Reports.Queries.GetAdvancedSalesReport;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Authorization;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Reports;

public static class GetAdvancedSalesReportEndpoint
{
    public static IEndpointRouteBuilder MapGetAdvancedSalesReportEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/seller/reports/advanced-sales",
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

                var query = new GetAdvancedSalesReportQuery(
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
            .WithName("GetAdvancedSalesReport")
            .WithTags("Seller Reports");

        return app;
    }
}
