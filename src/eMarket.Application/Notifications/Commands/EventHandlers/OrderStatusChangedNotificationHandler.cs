
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Notifications.Entities;
using eMarket.Domain.Notifications.Enums;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Sales.Orders.Events;
using MediatR;

namespace eMarket.Application.Notifications.EventHandlers;

public sealed class OrderStatusChangedNotificationHandler
    : INotificationHandler<OrderStatusChangedDomainEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly INotificationRealtimeSender _realtimeSender;

    public OrderStatusChangedNotificationHandler(
        INotificationDbContext dbContext,
        INotificationRealtimeSender realtimeSender)
    {
        _dbContext = dbContext;
        _realtimeSender = realtimeSender;
    }

    public async Task Handle(
        OrderStatusChangedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var (title, message) = notification.Status switch
        {
            OrderStatus.Paid =>
                ("Ödeme Onaylandı", "Ödemeniz başarıyla onaylandı."),

            OrderStatus.Processing =>
                ("Siparişiniz Hazırlanıyor", "Mağaza siparişinizi hazırlamaya başladı."),

            OrderStatus.Shipped =>
                ("Siparişiniz Kargoya Verildi", "Siparişiniz yola çıktı."),

            OrderStatus.Delivered =>
                ("Siparişiniz Teslim Edildi", "Siparişiniz başarıyla teslim edildi."),

            OrderStatus.Cancelled =>
                ("Siparişiniz İptal Edildi", "Siparişiniz iptal edildi."),

            _ =>
                ("Sipariş Güncellendi", "Siparişinizin durumu güncellendi.")
        };

        var userNotification = Notification.Create(
            notification.UserId,
            title,
            message,
            NotificationType.Order,
            "/customer/orders");

        _dbContext.Notifications.Add(userNotification);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _realtimeSender.SendToUserAsync(
            userNotification.UserId.Value.ToString(),
            new
            {
                id = userNotification.Id.Value,
                title = userNotification.Title,
                message = userNotification.Message,
                type = (int)userNotification.Type,
                targetUrl = userNotification.TargetUrl,
                isRead = userNotification.IsRead,
                createdAt = userNotification.CreatedAt,
                readAt = userNotification.ReadAt
            },
            cancellationToken);
    }
}
