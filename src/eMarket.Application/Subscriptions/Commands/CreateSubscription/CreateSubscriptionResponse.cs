namespace eMarket.Application.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionResponse(
    Guid Id,
    Guid BusinessId,
    DayOfWeek DeliveryDay,
    DateTime NextDeliveryDate,
    string Status,
    decimal WeeklyProductAmount,
    decimal WeeklyShippingAmount,
    decimal WeeklyTotalAmount,
    decimal MonthlyAmount);
