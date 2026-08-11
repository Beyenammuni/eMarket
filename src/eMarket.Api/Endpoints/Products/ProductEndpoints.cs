using eMarket.Application.Catalog.Products.Commands.ActivateProduct;
using eMarket.Application.Catalog.Products.Commands.AddStock;
using eMarket.Application.Catalog.Products.Commands.CreateProduct;
using eMarket.Application.Catalog.Products.Commands.DeactivateProduct;
using eMarket.Application.Catalog.Products.Commands.DeleteProduct;
using eMarket.Application.Catalog.Products.Commands.RemoveStock;
using eMarket.Application.Catalog.Products.Commands.UpdateProduct;
using eMarket.Application.Catalog.Products.Queries.GetProductById;
using eMarket.Application.Catalog.Products.Queries.GetProducts;
using eMarket.Domain.Businesses;
using MediatR;

namespace eMarket.Api.Endpoints.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");
        group.MapPatch("/{id:guid}/activate", ActivateProduct);

        group.MapPatch("/{id:guid}/deactivate", DeactivateProduct);

        group.MapPatch("/{id:guid}/stock/add", AddStock);

        group.MapPatch("/{id:guid}/stock/remove", RemoveStock);

        group.MapPost("/", CreateProduct);
        group.MapDelete(
    "/{id:guid}",
    DeleteProduct)
    .RequireAuthorization();
        group.MapGet("/", GetProducts);

        group.MapGet("/{id:guid}", GetProductById);
        group.MapPut("/{id:guid}", UpdateProduct);
        return app;
    }
    private static async Task<IResult> ActivateProduct(
    Guid id,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ActivateProductCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.NoContent();
    }
    private static async Task<IResult> DeactivateProduct(
    Guid id,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeactivateProductCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.NoContent();
    }
    private static async Task<IResult> AddStock(
    Guid id,
    BusinessId businessId,
    StockRequest request,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AddStockCommand(
                id,
                request.Quantity),
            cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.NoContent();
    }
    private static async Task<IResult> RemoveStock(
    Guid id,
    BusinessId businessId,
    StockRequest request,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RemoveStockCommand(
                id,
                request.Quantity),
            cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.NoContent();
    }
    private static async Task<IResult> CreateProduct(
        CreateProductCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error);
        }

        return Results.Created(
            $"/api/products/{result.Value.Id}",
            result.Value);
    }
    private static async Task<IResult> UpdateProduct(
    Guid id,
    UpdateProductCommand command,
    ISender sender,
    CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest(
                "Route id and request id must match.");
        }

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error);
        }

        return Results.Ok(result.Value);
    }
    private static async Task<IResult> GetProducts(
        [AsParameters] GetProductsQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductByIdQuery(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.NotFound(result.Error);
        }
        return Results.Ok(result.Value);
    }
    private static async Task<IResult> DeleteProduct(
    Guid id,
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteProductCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.NoContent();
    }
}
