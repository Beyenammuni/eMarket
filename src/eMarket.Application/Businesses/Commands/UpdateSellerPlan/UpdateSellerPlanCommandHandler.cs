using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses.Entities;
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

        if (request.Features is not null)
        {
            var existingFeatures = await _dbContext.SellerPlanFeatures
                .Where(x => x.SellerPlanId == request.Id)
                .ToListAsync(cancellationToken);

            _dbContext.SellerPlanFeatures.RemoveRange(existingFeatures);

            var newFeatures = request.Features
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(x => SellerPlanFeature.Create(request.Id, x));

            _dbContext.SellerPlanFeatures.AddRange(newFeatures);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
