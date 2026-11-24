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
        app.MapCreateBranchEndpoint();
        app.MapUpdateBranchEndpoint();
        app.MapDeactivateBranchEndpoint();
        app.MapActivateBranchEndpoint();
        app.MapAddBusinessMemberEndpoint();
        app.MapChangeMemberRoleEndpoint();
        app.MapSetMemberPermissionEndpoint();
        app.MapRemoveMemberEndpoint();
        app.MapTransferOwnershipEndpoint();
        app.MapUpdateBusinessEndpoint();
        app.MapSelectBusinessEndpoint();
        app.MapGetActiveBusinessesEndpoint();
        app.MapSuspendBusinessEndpoint();
        app.MapActivateBusinessEndpoint();
        app.MapSetBusinessSubscriptionSupportEndpoint();
        app.MapStartSellerPlanSubscriptionEndpoint();
        app.MapActivateFreeSellerPlanEndpoint();
        // Queries
        app.MapGetBusinessBranchesEndpoint();
        app.MapGetCurrentSellerPlanSubscriptionEndpoint();
        app.MapGetBusinessActivityLogs();
        app.MapGetSellerPlanFeaturesEndpoint();
        app.MapGetAvailableBusinessMembers();
        app.MapGetBusinessByIdEndpoint();
        app.MapGetMyBusinessesEndpoint();
        app.MapMapGetBusinessMembersEndpoint();
        app.MapGetAvailableSellerPlansEndpoint();
        return app;
    }
}
