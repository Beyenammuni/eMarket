using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Subscriptions;

public static class SubscriptionErrors
{
    public static readonly Error NotFound = new("Subscription.NotFound", "Subscription was not found.");
    public static readonly Error EmptyBasket = new("Subscription.EmptyBasket", "A subscription must contain at least one product.");
    public static readonly Error InvalidQuantity = new("Subscription.InvalidQuantity", "Quantity must be greater than zero.");
    public static readonly Error AlreadyCancelled = new("Subscription.AlreadyCancelled", "Subscription is already cancelled.");
    public static readonly Error NotActive = new("Subscription.NotActive", "Subscription is not active.");
    public static readonly Error InvalidDeliveryDay = new("Subscription.InvalidDeliveryDay", "Delivery day is invalid.");
    public static readonly Error DifferentBusinesses = new("Subscription.DifferentBusinesses", "All subscription products must belong to the selected business.");
}
