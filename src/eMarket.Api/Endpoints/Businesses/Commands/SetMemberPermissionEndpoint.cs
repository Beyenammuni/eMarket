
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Commands.SetMemberPermission;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses;

public static class SetMemberPermissionEndpoint
{
    public static IEndpointRouteBuilder MapSetMemberPermissionEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/businesses/{businessId:guid}/members/{userId:guid}/permissions",
            async (
                Guid businessId,
                Guid userId,
                SetMemberPermissionRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new SetMemberPermissionCommand(
                    businessId,
                    userId,
                    request.Permission,
                    request.IsGranted);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.NoContent();
            })
            .WithName("SetMemberPermission")
            .WithTags("Businesses")
           .RequireAuthorization();

        return app;
    }
}

public sealed record SetMemberPermissionRequest(
    string Permission,
    bool IsGranted);
