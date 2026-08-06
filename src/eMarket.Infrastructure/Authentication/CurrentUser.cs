using System.Security.Claims;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using Microsoft.AspNetCore.Http;

namespace eMarket.Infrastructure.Authentication;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? Email =>
        _httpContextAccessor.HttpContext?
            .User
            .FindFirst(ClaimTypes.Email)?.Value;

    public UserId UserId
    {
        get
        {
#if DEBUG
            var id = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return string.IsNullOrWhiteSpace(id)
                ? UserId.Create(Guid.Empty)
                : UserId.Create(Guid.Parse(id));
#else
        var id = _httpContextAccessor.HttpContext?
            .User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(id))
            throw new UnauthorizedAccessException();

        return UserId.Create(Guid.Parse(id));
#endif
        }
    }
}
