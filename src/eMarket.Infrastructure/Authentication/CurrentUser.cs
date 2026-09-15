using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace eMarket.Api.Common.Authentication;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;
    public IReadOnlyCollection<string> Roles =>
        User?.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? Array.Empty<string>();

    public BusinessRole? BusinessRole
    {
        get
        {
            var value =
                User?.FindFirst("businessRole")?.Value;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return BusinessRole.List
                .FirstOrDefault(x =>
                    x.Name.Equals(
                        value,
                        StringComparison.OrdinalIgnoreCase));
        }
    }
    public UserId? UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(value, out var id))
                return null;

            return UserId.Create(id);
        }
    }

    public BusinessId? BusinessId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
                return null;

            if (httpContext.Request.RouteValues.TryGetValue("businessId", out var routeValue) &&
                Guid.TryParse(routeValue?.ToString(), out var routeId))
            {
                return BusinessId.Create(routeId);
            }

            var header = httpContext.Request.Headers["X-Business-Id"].FirstOrDefault();
            if (Guid.TryParse(header, out var headerId))
                return BusinessId.Create(headerId);

            var value = User?.FindFirst("businessId")?.Value;
            if (!Guid.TryParse(value, out var id))
                return null;

            return BusinessId.Create(id);
        }
    }
    public string? Email =>
    User?.FindFirst(ClaimTypes.Email)?.Value;
    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) == true;
    }
}
