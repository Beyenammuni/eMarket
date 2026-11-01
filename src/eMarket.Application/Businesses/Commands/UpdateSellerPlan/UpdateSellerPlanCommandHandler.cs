
using eMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace eMarket.Application.Businesses.Commands.UpdateSellerPlan;

public sealed class UpdateSellerPlanCommandHandler
    : IRequestHandler<UpdateSellerPlanCommand, bool>
{
    private readonly IBusinessDbContext _dbContext;

    public UpdateSellerPlanCommandHandler(IBusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(
        UpdateSellerPlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _dbContext.SellerPlans
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (plan is null)
            return false;

        plan.Update(
            request.Name,
            request.Price,
            request.DurationDays,
            request.IsActive);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
