
using eMarket.Api.Common;
using eMarket.Application.Businesses.Queries.GetSellerDashboard;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Authorization;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Seller;

public static class GetSellerDashboardEndpoint
{
    public static IEndpointRouteBuilder MapGetSellerDashboardEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/seller/dashboard",
            async (
                int? days,
                Guid? businessId,
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

                var query = new GetSellerDashboardQuery(
                    days ?? 30,
                    businessId);

                var result = await sender.Send(
                    query,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetSellerDashboard")
            .WithTags("Seller Dashboard");

        return app;
    }
}
