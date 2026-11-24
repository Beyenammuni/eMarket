
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(
    Guid NotificationId
) : IRequest<Result>;
