using eMarket.Api.Common;
using eMarket.Application.Businesses.Commands.AddBusinessMember;
using MediatR;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;

namespace eMarket.Api.Endpoints.Businesses;

public static class AddBusinessMemberEndpoint
{
    public static IEndpointRouteBuilder MapAddBusinessMemberEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/businesses/{businessId:guid}/members",
            async (
                Guid businessId,
                AddBusinessMemberRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new AddBusinessMemberCommand(
                    businessId,
                    request.UserId,
                    request.Role);

                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("AddBusinessMember")
            .WithTags("Businesses")
             .RequireBusinessPermission(Permissions.Business.ManageMembers);

        return app;
    }
}
