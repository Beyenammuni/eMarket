using eMarket.Domain.Identity;
using eMarket.Domain.Notifications.Enums;
using eMarket.Domain.Notifications.ValueObjects;
using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Notifications.Entities;

public sealed class Notification : AggregateRoot<NotificationId>
{
    private Notification()
    {
    }

    private Notification(
        NotificationId id,
        UserId userId,
        string title,
        string message,
        NotificationType type,
        string? targetUrl)
    {
        Id = id;
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
        TargetUrl = targetUrl;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public UserId UserId { get; private set; } = default!;

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public NotificationType Type { get; private set; }

    public string? TargetUrl { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public static Notification Create(
        UserId userId,
        string title,
        string message,
        NotificationType type,
        string? targetUrl = null)
    {
        ArgumentNullException.ThrowIfNull(userId);

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Notification title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notification message is required.", nameof(message));

        return new Notification(
            NotificationId.New(),
            userId,
            title.Trim(),
            message.Trim(),
            type,
            targetUrl);
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
