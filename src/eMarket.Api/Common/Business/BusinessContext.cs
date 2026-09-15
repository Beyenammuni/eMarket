using eMarket.Application.Catalog.Categories.Commands.UpdateCategory;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using Microsoft.AspNetCore.Http;

namespace eMarket.Api.Common.Business;

public sealed class BusinessContext : IBusinessContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BusinessContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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

            var jwtValue = httpContext.User.FindFirst("businessId")?.Value;
            if (Guid.TryParse(jwtValue, out var jwtId))
                return BusinessId.Create(jwtId);

            return null;
        }
    }

    public bool HasBusiness => BusinessId is not null;
}
