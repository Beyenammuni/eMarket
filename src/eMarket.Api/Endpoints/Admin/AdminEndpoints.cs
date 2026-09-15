using eMarket.Api.Common;
using eMarket.Application.Admin.Queries.GetDashboard;
using MediatR;
using eMarket.SharedKernel.Constants;
using eMarket.Api.Common.Authorization;

namespace eMarket.Api.Endpoints.Admin;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireRoles(Roles.Admin, Roles.SuperAdmin);

        group.MapGet("/dashboard", async (int? days, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAdminDashboardQuery(days ?? 30), cancellationToken);
            return result.IsFailure ? ApiResults.Failure(result.Error) : Results.Ok(result.Value);
        })
        .WithName("GetAdminDashboard")
        .Produces(200)
        .Produces(401)
        .Produces(403);

        return app;
    }
}
