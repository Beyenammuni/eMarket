using eMarket.Api.Common;
using eMarket.Application.Businesses.Queries.GetSellerDashboard;
using eMarket.Application.Common.Interfaces;
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
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetSellerDashboardQuery(
                    days ?? 30);

                var result = await sender.Send(
                    query,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetSellerDashboard")
            .WithTags("Seller Dashboard")
            .AddEndpointFilter(async (context, next) =>
            {
                var currentUser =
                    context.HttpContext.RequestServices
                        .GetRequiredService<ICurrentUser>();

                if (currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                var isGlobalAdmin =
                    currentUser.IsInRole(Roles.Admin) ||
                    currentUser.IsInRole(Roles.SuperAdmin);

                if (isGlobalAdmin)
                    return await next(context);

                var authorization =
                    context.HttpContext.RequestServices
                        .GetRequiredService<IBusinessAuthorization>();

                var allowed =
                    await authorization.HasPermissionOnAnyBusinessAsync(
                        Permissions.Business.View,
                        context.HttpContext.RequestAborted);

                if (!allowed)
                    return ApiResults.Forbidden();

                return await next(context);
            });

        return app;
    }
}
