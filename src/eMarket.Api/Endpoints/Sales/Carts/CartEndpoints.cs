using eMarket.Api.Common;
using eMarket.Application.Sales.Carts.Commands.AddToCart;
using eMarket.Application.Sales.Carts.Commands.ClearCart;
using eMarket.Application.Sales.Carts.Commands.RemoveFromCart;
using eMarket.Application.Sales.Carts.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;
using eMarket.SharedKernel.Constants;

namespace eMarket.Api.Endpoints.Sales.Carts;

public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Cart");

        group.MapGet(
            "/",
            GetCart)
            .RequireRoles(Roles.Customer);

        group.MapPost(
            "/items",
            AddToCart)
            .RequireRoles(Roles.Customer);

        group.MapDelete(
            "/items/{productId:guid}",
            RemoveFromCart)
            .RequireRoles(Roles.Customer);

        group.MapDelete(
            "/",
            ClearCart)
            .RequireRoles(Roles.Customer);

        return app;
    }

    private static async Task<IResult> GetCart(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCartQuery(),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> AddToCart(
        [FromBody] AddToCartCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveFromCart(
        Guid productId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RemoveFromCartCommand(productId),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> ClearCart(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ClearCartCommand(),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.NoContent();
    }
}
