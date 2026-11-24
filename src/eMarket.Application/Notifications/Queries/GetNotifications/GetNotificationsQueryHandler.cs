
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, Result<NotificationsResponse>>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetNotificationsQueryHandler(
        INotificationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<NotificationsResponse>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result<NotificationsResponse>.Failure(
                new Error("Notifications.Unauthorized",
                    "User is not authenticated."));
        }

        if (request.Page < 1 ||
            request.PageSize < 1 ||
            request.PageSize > 100)
        {
            return Result<NotificationsResponse>.Failure(
                new Error("Notifications.InvalidPagination",
                    "Invalid pagination values."));
        }

        var userId = _currentUser.UserId;

        var notifications = _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        var totalCount = await notifications.CountAsync(
            cancellationToken);

        var unreadCount = await notifications.CountAsync(
            x => !x.IsRead,
            cancellationToken);

        var items = await notifications
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new NotificationItem(
                x.Id.Value,
                x.Title,
                x.Message,
                (int)x.Type,
                x.TargetUrl,
                x.IsRead,
                x.CreatedAt,
                x.ReadAt))
            .ToListAsync(cancellationToken);

        return Result<NotificationsResponse>.Success(
            new NotificationsResponse(
                unreadCount,
                totalCount,
                request.Page,
                request.PageSize,
                items));
    }
}
