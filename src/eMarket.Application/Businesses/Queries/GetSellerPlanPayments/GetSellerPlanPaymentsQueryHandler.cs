using eMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerPlanPayments;

public sealed class GetSellerPlanPaymentsQueryHandler
    : IRequestHandler<
        GetSellerPlanPaymentsQuery,
        IReadOnlyList<SellerPlanPaymentDto>>
{
    private readonly IBusinessDbContext _dbContext;

    public GetSellerPlanPaymentsQueryHandler(
        IBusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SellerPlanPaymentDto>> Handle(
        GetSellerPlanPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        return await (
            from payment in _dbContext.SellerPlanPayments.AsNoTracking()
            join business in _dbContext.Businesses.AsNoTracking()
                on payment.BusinessId equals business.Id
            join plan in _dbContext.SellerPlans.AsNoTracking()
                on payment.SellerPlanId equals plan.Id
            orderby payment.CreatedAt descending
            select new SellerPlanPaymentDto(
                payment.Id,
                payment.BusinessId.Value,
                business.Name.Value,
                plan.Id,
                plan.Name,
                payment.SellerPlanSubscriptionId,
                payment.Amount,
                payment.Currency,
                payment.Status,
                payment.CreatedAt,
                payment.VerifiedAt
            )
        ).ToListAsync(cancellationToken);
    }
}
