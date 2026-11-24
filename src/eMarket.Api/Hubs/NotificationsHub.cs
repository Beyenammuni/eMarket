
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace eMarket.Api.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                NotificationHubGroups.ForUser(userId));
        }

        await base.OnConnectedAsync();
    }
}

public static class NotificationHubGroups
{
    public static string ForUser(string userId)
        => $"user:{userId}";
}
