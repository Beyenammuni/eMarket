namespace eMarket.Application.Businesses.Queries.GetCurrentSellerPlanSubscription;

public sealed record CurrentSellerPlanSubscriptionDto(
    Guid SubscriptionId,
    Guid SellerPlanId,
    string PlanCode,
    string PlanName,
    decimal PlanPrice,
    string Currency,
    string Status,
    DateTime StartedAt,
    DateTime ExpiresAt,
    bool IsPro,
    bool IsCurrentlyActive);
