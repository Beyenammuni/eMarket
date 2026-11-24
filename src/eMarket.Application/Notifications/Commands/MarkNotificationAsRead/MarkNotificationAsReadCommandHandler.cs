
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Notifications.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Notifications.Commands.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public MarkNotificationAsReadCommandHandler(
        INotificationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Result.Failure(
                new Error("Notifications.Unauthorized",
                    "User is not authenticated."));

        if (request.NotificationId == Guid.Empty)
            return Result.Failure(
                new Error("Notifications.InvalidId",
                    "Notification ID is invalid."));

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                x => x.Id == NotificationId.Create(request.NotificationId)
                     && x.UserId == _currentUser.UserId,
                cancellationToken);

        if (notification is null)
            return Result.Failure(
                new Error("Notifications.NotFound",
                    "Notification was not found."));

        notification.MarkAsRead();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
