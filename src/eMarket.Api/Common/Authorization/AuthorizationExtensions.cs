using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Constants;
using eMarket.SharedKernel.Results;

namespace eMarket.Api.Common.Authorization;

public static class AuthorizationExtensions
{
    public static RouteHandlerBuilder RequireRoles(
        this RouteHandlerBuilder endpoint,
        params string[] roles)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (roles is null || roles.Length == 0)
            throw new ArgumentException("At least one role is required.", nameof(roles));

        return endpoint.RequireAuthorization(policy =>
            policy.RequireAuthenticatedUser()
                  .RequireRole(roles));
    }

    public static RouteGroupBuilder RequireRoles(
        this RouteGroupBuilder group,
        params string[] roles)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (roles is null || roles.Length == 0)
            throw new ArgumentException("At least one role is required.", nameof(roles));

        return group.RequireAuthorization(policy =>
            policy.RequireAuthenticatedUser()
                  .RequireRole(roles));
    }

    /// <summary>
    /// Requires an authenticated user and a selected business context.
    /// Business context is resolved consistently from route, X-Business-Id, or JWT.
    /// Global Admin/SuperAdmin users bypass membership/permission checks, but still
    /// need a business context for business-scoped commands that operate on a business.
    /// </summary>
    public static RouteHandlerBuilder RequireBusinessPermission(
        this RouteHandlerBuilder endpoint,
        string permission)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return endpoint
            .RequireAuthorization(policy => policy.RequireAuthenticatedUser())
            .AddEndpointFilter(async (context, next) =>
            {
                var httpContext = context.HttpContext;

                if (httpContext.User.Identity?.IsAuthenticated != true)
                    return ApiResults.Unauthorized();

                var currentUser =
                    httpContext.RequestServices.GetRequiredService<ICurrentUser>();

                if (currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                var businessId = ResolveBusinessId(httpContext, currentUser);
                if (businessId is null)
                {
                    return ApiResults.Failure(
                        new Error(
                            "Business.Context.Required",
                            "A business context is required. Select a business or send a valid X-Business-Id header."));
                }

                var isGlobalAdmin =
                    currentUser.IsInRole(Roles.SuperAdmin) ||
                    currentUser.IsInRole(Roles.Admin);

                if (!isGlobalAdmin)
                {
                    var authorization =
                        httpContext.RequestServices.GetRequiredService<IBusinessAuthorization>();

                    var allowed = await authorization.HasPermissionAsync(
                        businessId,
                        permission,
                        httpContext.RequestAborted);

                    if (!allowed)
                        return ApiResults.Forbidden();
                }

                return await next(context);
            });
    }

    private static eMarket.Domain.Businesses.BusinessId? ResolveBusinessId(
        HttpContext httpContext,
        ICurrentUser currentUser)
    {
        if (httpContext.Request.RouteValues.TryGetValue("businessId", out var routeValue) &&
            Guid.TryParse(routeValue?.ToString(), out var routeId))
        {
            return eMarket.Domain.Businesses.BusinessId.Create(routeId);
        }

        var header = httpContext.Request.Headers["X-Business-Id"].FirstOrDefault();
        if (Guid.TryParse(header, out var headerId))
            return eMarket.Domain.Businesses.BusinessId.Create(headerId);

        return currentUser.BusinessId;
    }
}
