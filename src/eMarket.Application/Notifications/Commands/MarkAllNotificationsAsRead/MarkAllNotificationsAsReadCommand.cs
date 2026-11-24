
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed record MarkAllNotificationsAsReadCommand
    : IRequest<Result>;
