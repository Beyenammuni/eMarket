
using eMarket.Api.Common;
using eMarket.SharedKernel.Constants;

namespace eMarket.Api.Endpoints.Admin;

public static class AssignSellerRoleEndpoint
{
    public static IEndpointRouteBuilder MapAssignSellerRoleEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/admin/users/{userId:guid}/seller-role",
            () => Results.Conflict(new
            {
                code = "SellerRole.AssignmentDisabled",
                message = "Seller role is assigned automatically after business approval and successful payment setup."
            }))
            .WithName("AssignSellerRole")
            .WithTags("Admin Users")
            .RequireAuthorization(policy =>
                policy.RequireRole(
                    Roles.Admin,
                    Roles.SuperAdmin));

        return app;
    }
}
