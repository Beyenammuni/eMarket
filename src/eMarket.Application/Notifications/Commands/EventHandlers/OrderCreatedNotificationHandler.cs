
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Notifications.Entities;
using eMarket.Domain.Notifications.Enums;
using eMarket.Domain.Sales.Orders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Notifications.Commands.EventHandlers;

public sealed class OrderCreatedNotificationHandler
    : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly INotificationRealtimeSender _realtimeSender;

    public OrderCreatedNotificationHandler(
        INotificationDbContext dbContext,
        INotificationRealtimeSender realtimeSender)
    {
        _dbContext = dbContext;
        _realtimeSender = realtimeSender;
    }

    public async Task Handle(
        OrderCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == notification.OrderId,
                cancellationToken);

        if (order is null)
            return;

        var members = await _dbContext.BusinessMembers
            .Where(x =>
                x.BusinessId == order.BusinessId &&
                x.IsActive)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var createdNotifications = new List<Notification>();

        foreach (var userId in members)
        {
            var userNotification = Notification.Create(
                userId,
                "Yeni sipariş aldınız",
                "Mağazanıza yeni bir sipariş ulaştı.",
                NotificationType.Order,
                "/orders");

            _dbContext.Notifications.Add(userNotification);
            createdNotifications.Add(userNotification);
        }

        if (createdNotifications.Count == 0)
            return;

        // Save notifications before sending real-time messages.
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var userNotification in createdNotifications)
        {
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
}
