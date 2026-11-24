using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Promotions;

public static class PromotionErrors
{
    public static readonly Error BusinessRequired = new(
        "Promotion.BusinessRequired",
        "Business is required.");

    public static readonly Error Forbidden = new(
        "Promotion.Forbidden",
        "You do not have permission to manage promotions.");

    public static readonly Error ProductsRequired = new(
        "Promotion.ProductsRequired",
        "At least one product is required.");

    public static readonly Error ProductsNotFound = new(
        "Promotion.ProductsNotFound",
        "One or more products do not exist or do not belong to this business.");

    public static readonly Error ProductNotActive = new(
        "Promotion.ProductNotActive",
        "All promotion products must be active.");

    public static readonly Error OverlappingPromotion = new(
        "Promotion.Overlapping",
        "One or more products already have an overlapping promotion.");

    public static readonly Error InvalidPromotion = new(
        "Promotion.Invalid",
        "Promotion data is invalid.");
    public static readonly Error BusinessNotActive = new(
    "Promotion.BusinessNotActive",
    "The business must be active to create promotions.");
    public static readonly Error PromotionNotFound = new(
    "Promotion.NotFound",
    "Promotion was not found.");
}
