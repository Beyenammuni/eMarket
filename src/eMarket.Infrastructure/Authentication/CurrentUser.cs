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
            var value = User?.FindFirst("businessId")?.Value;

            if (!Guid.TryParse(value, out var id))
                return null;

            return BusinessId.Create(id);
        }
    }

    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) == true;
    }
}
