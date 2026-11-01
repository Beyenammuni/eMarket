
using eMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerPlans;

public sealed class GetSellerPlansQueryHandler
    : IRequestHandler<GetSellerPlansQuery, IReadOnlyList<SellerPlanDto>>
{
    private readonly IBusinessDbContext _dbContext;

    public GetSellerPlansQueryHandler(IBusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SellerPlanDto>> Handle(
        GetSellerPlansQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.SellerPlans
            .AsNoTracking()
            .AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(plan => plan.IsActive);
        }

        return await query
            .OrderBy(plan => plan.Price)
            .Select(plan => new SellerPlanDto(
                plan.Id,
                plan.Code,
                plan.Name,
                plan.Price,
                plan.Currency,
                plan.DurationDays,
                plan.IsActive,
                plan.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
