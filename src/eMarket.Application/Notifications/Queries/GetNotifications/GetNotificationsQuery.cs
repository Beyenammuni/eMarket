
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<NotificationsResponse>>;

public sealed record NotificationsResponse(
    int UnreadCount,
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<NotificationItem> Items
);

public sealed record NotificationItem(
    Guid Id,
    string Title,
    string Message,
    int Type,
    string? TargetUrl,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt
);
