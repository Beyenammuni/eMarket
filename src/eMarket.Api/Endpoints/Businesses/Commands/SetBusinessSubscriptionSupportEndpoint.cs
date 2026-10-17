using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Commands.SetBusinessSubscriptionSupport;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class SetBusinessSubscriptionSupportEndpoint
{
    public static IEndpointRouteBuilder MapSetBusinessSubscriptionSupportEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/businesses/{businessId:guid}/subscription-support",
            async (
                Guid businessId,
                SetBusinessSubscriptionSupportRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new SetBusinessSubscriptionSupportCommand(
                    businessId,
                    request.Enabled);

                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(new { supportsSubscriptions = result.Value });
            })
            .WithName("SetBusinessSubscriptionSupport")
            .WithTags("Businesses")
            .RequireBusinessPermission(Permissions.Business.Update);

        return app;
    }
}

public sealed record SetBusinessSubscriptionSupportRequest(bool Enabled);
