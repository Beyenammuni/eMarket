using eMarket.Application.Common.Models;
using eMarket.SharedKernel.Pagination;

namespace eMarket.Infrastructure.Persistence.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        PaginationRequest request)
    {
        return query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);
    }
}
