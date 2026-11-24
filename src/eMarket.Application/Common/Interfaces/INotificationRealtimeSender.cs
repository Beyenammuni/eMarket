namespace eMarket.Application.Common.Interfaces;

public interface INotificationRealtimeSender
{
    Task SendToUserAsync(
        string userId,
        object notification,
        CancellationToken cancellationToken = default);
}
