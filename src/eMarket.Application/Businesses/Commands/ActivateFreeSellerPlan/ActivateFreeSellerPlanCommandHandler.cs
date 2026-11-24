using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Commands.ActivateFreeSellerPlan;

public sealed class ActivateFreeSellerPlanCommandHandler
    : IRequestHandler<ActivateFreeSellerPlanCommand, Guid>
{
    private readonly IBusinessDbContext _businessDb;
    private readonly ICurrentUser _currentUser;

    public ActivateFreeSellerPlanCommandHandler(
        IBusinessDbContext businessDb,
        ICurrentUser currentUser)
    {
        _businessDb = businessDb;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        ActivateFreeSellerPlanCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var businessId = BusinessId.Create(request.BusinessId);

        var businessExists = await _businessDb.Businesses
            .AnyAsync(x => x.Id == businessId, cancellationToken);

        if (!businessExists)
            throw new InvalidOperationException("Business not found.");

        var isOwner = await _businessDb.BusinessMembers.AnyAsync(
    x => x.BusinessId == businessId &&
         x.UserId == userId &&
         x.IsActive &&
         (x.Role == BusinessRole.Owner ||
          x.Role == BusinessRole.Manager),
    cancellationToken);

        if (!isOwner)
            throw new UnauthorizedAccessException(
                "Only the business owner can activate a seller plan.");

        var plan = await _businessDb.SellerPlans
            .FirstOrDefaultAsync(
                x => x.Id == request.SellerPlanId && x.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "Seller plan not found or inactive.");

        if (plan.Price != 0 || plan.DurationDays != 0)
            throw new InvalidOperationException(
                "This plan is not a free permanent plan.");

        var now = DateTime.UtcNow;

        var hasActiveSubscription = await _businessDb.SellerPlanSubscriptions
            .AnyAsync(
                x => x.BusinessId == businessId &&
                     x.Status == "Active" &&
                     x.ExpiresAt > now,
                cancellationToken);

        if (hasActiveSubscription)
            throw new InvalidOperationException(
                "The business already has an active subscription.");

        var subscription = SellerPlanSubscription.CreatePending(
            businessId,
            plan.Id);

        subscription.ActivateFree();

        _businessDb.SellerPlanSubscriptions.Add(subscription);

        await _businessDb.SaveChangesAsync(cancellationToken);

        return subscription.Id;
    }
}
