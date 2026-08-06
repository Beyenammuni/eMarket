using eMarket.Api.Endpoints.Businesses;
using eMarket.Api.Endpoints.Businesses.Commands;
using eMarket.Api.Endpoints.Businesses.Queries;

public static class BusinessEndpoints
{
    public static IEndpointRouteBuilder MapBusinessEndpoints(
        this IEndpointRouteBuilder app)
    {
        // Commands
        app.MapCreateBusinessEndpoint();
        app.MapAddBusinessMemberEndpoint();
        app.MapChangeMemberRoleEndpoint();
        app.MapRemoveMemberEndpoint();
        app.MapTransferOwnershipEndpoint();

        // Queries
        app.MapGetBusinessByIdEndpoint();
        app.MapGetMyBusinessesEndpoint();
        app.MapMapGetBusinessMembersEndpoint();

        return app;
    }
}
