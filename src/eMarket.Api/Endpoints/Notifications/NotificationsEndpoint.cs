
using eMarket.Api.Common;
using eMarket.Application.Notifications.Queries.GetNotifications;
using eMarket.Application.Notifications.Commands.MarkNotificationAsRead;
using eMarket.Application.Notifications.Commands.MarkAllNotificationsAsRead;
using MediatR;

namespace eMarket.Api.Endpoints.Notifications;

public static class NotificationsEndpoint
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetNotificationsQuery(
                    page ?? 1,
                    pageSize ?? 20),
                cancellationToken);

            return result.IsFailure
                ? ApiResults.Failure(result.Error)
                : Results.Ok(result.Value);
        })
        .WithName("GetNotifications")
        .Produces<NotificationsResponse>(200)
        .Produces(401);

        group.MapGet("/unread-count", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetNotificationsQuery(1, 1),
                cancellationToken);

            return result.IsFailure
                ? ApiResults.Failure(result.Error)
                : Results.Ok(result.Value.UnreadCount);
        })
        .WithName("GetUnreadNotificationsCount")
        .Produces<int>(200)
        .Produces(401);

        group.MapPatch("/{notificationId:guid}/read", async (
            Guid notificationId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new MarkNotificationAsReadCommand(notificationId),
                cancellationToken);

            return result.IsFailure
                ? ApiResults.Failure(result.Error)
                : Results.NoContent();
        })
        .WithName("MarkNotificationAsRead")
        .Produces(204)
        .Produces(401)
        .Produces(404);

        group.MapPatch("/read-all", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new MarkAllNotificationsAsReadCommand(),
                cancellationToken);

            return result.IsFailure
                ? ApiResults.Failure(result.Error)
                : Results.NoContent();
        })
        .WithName("MarkAllNotificationsAsRead")
        .Produces(204)
        .Produces(401);

        return app;
    }
}
