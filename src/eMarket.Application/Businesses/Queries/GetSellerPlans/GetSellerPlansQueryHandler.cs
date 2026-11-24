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
            query = query.Where(plan => plan.IsActive);

        var plans = await query
            .OrderBy(plan => plan.Price)
            .ToListAsync(cancellationToken);

        var planIds = plans.Select(x => x.Id).ToList();

        var features = await _dbContext.SellerPlanFeatures
            .AsNoTracking()
            .Where(x => planIds.Contains(x.SellerPlanId))
            .ToListAsync(cancellationToken);

        return plans.Select(plan => new SellerPlanDto(
            plan.Id,
            plan.Code,
            plan.Name,
            plan.Price,
            plan.Currency,
            plan.DurationDays,
            plan.IsActive,
            plan.UpdatedAt,
            features
                .Where(x => x.SellerPlanId == plan.Id)
                .Select(x => x.Code)
                .ToList()
        )).ToList();
    }
}
