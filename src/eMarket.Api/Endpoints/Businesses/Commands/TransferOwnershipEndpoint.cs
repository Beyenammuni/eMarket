using eMarket.Api.Common;
using eMarket.Application.Businesses.Commands.TransferOwnership;
using MediatR;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class TransferOwnershipEndpoint
{
    public static IEndpointRouteBuilder MapTransferOwnershipEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/businesses/{businessId:guid}/transfer-ownership",
            async (
                Guid businessId,
                TransferOwnershipRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new TransferOwnershipCommand(
                    businessId,
                    request.NewOwnerId);

                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("TransferOwnership")
            .WithTags("Businesses")
             .RequireBusinessPermission(Permissions.Business.TransferOwnership);

        return app;
    }
}
