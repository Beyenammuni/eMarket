using eMarket.Api.Common;
using eMarket.Application.Catalog.Products.Queries.GetAdvancedInventory;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Inventory;

public static class GetAdvancedInventoryEndpoint
{
    public static IEndpointRouteBuilder MapGetAdvancedInventoryEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/seller/inventory/advanced",
            async (
                Guid? businessId,
                int page,
                int pageSize,
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

                var query = new GetAdvancedInventoryQuery(
                    businessId.Value,
                    page <= 0 ? 1 : page,
                    pageSize <= 0 ? 20 : pageSize);

                var result = await sender.Send(
                    query,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetAdvancedInventory")
            .WithTags("Seller Inventory");

        return app;
    }
}
