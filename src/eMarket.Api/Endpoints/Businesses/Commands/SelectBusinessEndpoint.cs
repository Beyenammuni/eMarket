using eMarket.Api.Common;
using eMarket.Application.Businesses.Commands.SelectBusiness;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class SelectBusinessEndpoint
{
    public static IEndpointRouteBuilder MapSelectBusinessEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/businesses/{businessId:guid}/select",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new SelectBusinessCommand(businessId),
                    cancellationToken);

                return result.IsFailure
                    ? ApiResults.Failure(result.Error)
                    : Results.Ok(result.Value);
            })
            .WithName("SelectBusiness")
            .WithTags("Businesses")
            .RequireAuthorization(policy =>
                policy.RequireRole(
                    Roles.Seller,
                    Roles.StoreManager,
                    Roles.Admin,
                    Roles.SuperAdmin));

        return app;
    }
}
