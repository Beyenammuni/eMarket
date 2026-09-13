using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Sales.Orders.Commands.Cancel;
using eMarket.Application.Sales.Orders.Commands.CreateOrder;
using eMarket.Application.Sales.Orders.Commands.Deliver;
using eMarket.Application.Sales.Orders.Commands.MarkAsPaid;
using eMarket.Application.Sales.Orders.Commands.Ship;
using eMarket.Application.Sales.Orders.Commands.StartProcessing;
using eMarket.Application.Sales.Orders.Queries.GetMyOrders;
using eMarket.Application.Sales.Orders.Queries.GetOrderById;
using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapPost(
            "/",
            CreateOrder)
            .RequireRoles(Roles.Customer);

        group.MapGet(
            "/",
            GetMyOrders)
            .RequireRoles(Roles.Customer);

        group.MapGet(
            "/{id:guid}",
            GetOrderById)
            .RequireRoles(Roles.Customer);

        group.MapPatch(
    "/{id:guid}/pay",
    MarkAsPaid)
            .RequireRoles(Roles.Customer);

        group.MapPatch("/{id:guid}/processing",
    StartProcessing)
            .RequireRoles(Roles.Seller,
            Roles.StoreManager,
            Roles.Admin,
            Roles.SuperAdmin);

        group.MapPatch(
    "/{id:guid}/ship",
    Ship)
            .RequireRoles(Roles.Seller,
            Roles.StoreManager,
            Roles.Admin,
            Roles.SuperAdmin);

        group.MapPatch(
    "/{id:guid}/deliver",
    Deliver).RequireRoles(Roles.Seller,
            Roles.StoreManager,
            Roles.DeliveryDriver,
            Roles.Admin,
            Roles.SuperAdmin);

        group.MapPatch(
    "/{id:guid}/cancel",
    Cancel).RequireRoles(Roles.Customer,
            Roles.Seller,
            Roles.StoreManager,
            Roles.Admin,
            Roles.SuperAdmin);

        return app;
    }
    private static async Task<IResult> Cancel(
    Guid id,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CancelOrderCommand(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.NoContent();
    }
    private static async Task<IResult> MarkAsPaid(
    Guid id,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new MarkAsPaidCommand(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> CreateOrder(
      CreateOrderRequest request,
      ISender sender,
      CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateOrderCommand(
                request.FullName,
                request.PhoneNumber,
                request.AddressLine,
                request.City,
                request.District,
                request.PostalCode,
                request.Neighborhood,
                request.Street,
                request.BuildingNumber,
                request.ApartmentNumber,
                request.Latitude,
                request.Longitude),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Created(
            $"/api/orders/{result.Value.Id}",
            result.Value);
    }
    private static async Task<IResult> StartProcessing(
    Guid id,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new StartProcessingCommand(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> Ship(
    Guid id,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ShipCommand(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.NoContent();
    }
    private static async Task<IResult> Deliver(
    Guid id,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeliverCommand(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.NoContent();
    }
    private static async Task<IResult> GetMyOrders(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetMyOrdersQuery(),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetOrderById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOrderByIdQuery(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }
}
public sealed record CreateOrderRequest(
   string FullName,
    string PhoneNumber,
    string AddressLine,
    string City,
    string District,
    string PostalCode,
    string Neighborhood,
    string Street,
    string BuildingNumber,
    string ApartmentNumber,
    decimal Latitude,
    decimal Longitude);
