using eMarket.Api.Common;
using eMarket.Application.Identity.Auth.Commands.AssignSellerRole;
using eMarket.Application.Identity.Commands.AssignSellerRole;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Admin;

public static class AssignSellerRoleEndpoint
{
    public static IEndpointRouteBuilder MapAssignSellerRoleEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/admin/users/{userId:guid}/seller-role",
            async (
                Guid userId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new AssignSellerRoleCommand(userId),
                    cancellationToken);

                if (result.IsFailure)
                {
                    return ApiResults.Failure(result.Error);
                }

                return Results.NoContent();
            })
            .WithName("AssignSellerRole")
            .WithTags("Admin Users")
            .RequireAuthorization(policy =>
                policy.RequireRole(
                    Roles.Admin,
                    Roles.SuperAdmin));

        return app;
    }
}
