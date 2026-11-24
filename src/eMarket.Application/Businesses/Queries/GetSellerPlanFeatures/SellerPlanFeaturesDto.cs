namespace eMarket.Application.Businesses.Queries.GetSellerPlanFeatures;

public sealed record SellerPlanFeaturesDto(
    DashboardFeaturesDto Dashboard,
    ReportsFeaturesDto Reports,
    OrdersFeaturesDto Orders,
    InventoryFeaturesDto Inventory,
    ProductsFeaturesDto Products,
    StaffFeaturesDto Staff,
    BranchesFeaturesDto Branches,
    SubscriptionsFeaturesDto Subscriptions);

public sealed record DashboardFeaturesDto(
    bool AdvancedAnalytics,
    bool TopProducts,
    bool ExtendedPeriod);

public sealed record ReportsFeaturesDto(
    bool AdvancedSales,
    bool StorePerformance);

public sealed record OrdersFeaturesDto(bool AdvancedAnalytics);

public sealed record InventoryFeaturesDto(bool AdvancedManagement);

public sealed record ProductsFeaturesDto(bool AdvancedManagement);

public sealed record StaffFeaturesDto(bool AdvancedManagement);

public sealed record BranchesFeaturesDto(bool Management);

public sealed record SubscriptionsFeaturesDto(bool Management);
