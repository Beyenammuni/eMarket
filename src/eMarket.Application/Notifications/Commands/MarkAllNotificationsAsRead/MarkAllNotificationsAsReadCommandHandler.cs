
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsAsReadCommand, Result>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Result.Failure(
                new Error("Notifications.Unauthorized",
                    "User is not authenticated."));

        var notifications = await _dbContext.Notifications
            .Where(x =>
                x.UserId == _currentUser.UserId &&
                !x.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
            notification.MarkAsRead();

        if (notifications.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
