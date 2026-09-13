using eMarket.Api.Common;
using eMarket.Application.Businesses.Commands.UpdateBusiness;
using MediatR;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;

namespace eMarket.Api.Endpoints.Businesses.Commands
{
    public static class UpdateBusinessEndpoint
    {
        public static IEndpointRouteBuilder MapUpdateBusinessEndpoint(
    this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/businesses/{businessId:guid}",
    async (
    Guid businessId,
    UpdateBusinessRequest request,
    ISender sender,
    CancellationToken cancellationToken) =>
    {
        var command = new UpdateBusinessCommand(
            businessId,
            request.Name,
            request.Type);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return ApiResults.Failure(result.Error);

        return Results.Ok(result.Value);
    })
    .WithName("UpdateBusiness")
    .WithTags("Businesses")
             .RequireBusinessPermission(Permissions.Business.Update);
            return app;
        }
    }
}
