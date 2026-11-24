using eMarket.Api.Hubs;
using eMarket.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace eMarket.Api.Services;

public sealed class SignalRNotificationRealtimeSender
    : INotificationRealtimeSender
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public SignalRNotificationRealtimeSender(
        IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendToUserAsync(
        string userId,
        object notification,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(NotificationHubGroups.ForUser(userId))
            .SendAsync(
                "NotificationReceived",
                notification,
                cancellationToken);
    }
}
