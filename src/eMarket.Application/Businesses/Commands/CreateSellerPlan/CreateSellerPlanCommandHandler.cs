using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Commands.CreateSellerPlan;

public sealed class CreateSellerPlanCommandHandler
    : IRequestHandler<CreateSellerPlanCommand, Guid>
{
    private readonly IBusinessDbContext _dbContext;

    public CreateSellerPlanCommandHandler(IBusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(
        CreateSellerPlanCommand request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        var exists = await _dbContext.SellerPlans
            .AnyAsync(
                plan => plan.Code == code,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                $"Seller plan code '{code}' already exists.");

        var features = (request.Features ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var plan = SellerPlan.Create(
            code,
            request.Name,
            request.Price,
            request.DurationDays,
            request.Currency);

        _dbContext.SellerPlans.Add(plan);

        var planFeatures = features.Select(feature =>
            SellerPlanFeature.Create(plan.Id, feature));

        _dbContext.SellerPlanFeatures.AddRange(planFeatures);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
