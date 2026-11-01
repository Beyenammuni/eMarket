using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Catalog.Products.Commands.ActivateProduct;
using eMarket.Application.Catalog.Products.Commands.AddProductOption;
using eMarket.Application.Catalog.Products.Commands.AddProductOptionGroup;
using eMarket.Application.Catalog.Products.Commands.AddProductVariant;
using eMarket.Application.Catalog.Products.Commands.AddStock;
using eMarket.Application.Catalog.Products.Commands.CreateProduct;
using eMarket.Application.Catalog.Products.Commands.DeactivateProduct;
using eMarket.Application.Catalog.Products.Commands.DeleteProduct;
using eMarket.Application.Catalog.Products.Commands.RemoveStock;
using eMarket.Application.Catalog.Products.Commands.UpdateProduct;
using eMarket.Application.Catalog.Products.Queries.GetProductById;
using eMarket.Application.Catalog.Products.Queries.GetProducts;
using eMarket.Application.Common.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace eMarket.Api.Endpoints.Catalog.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapPatch(
            "/{id:guid}/activate",
            ActivateProduct)
            .RequireBusinessPermission(Permissions.Products.Activate);

        group.MapPost(
    "/{id:guid}/option-groups/{optionGroupId:guid}/options",
    AddProductOption)
    .RequireBusinessPermission(Permissions.Products.Update);

        group.MapPatch(
            "/{id:guid}/deactivate",
            DeactivateProduct)
            .RequireBusinessPermission(Permissions.Products.Deactivate);

        group.MapPatch(
            "/{id:guid}/stock/add",
            AddStock)
            .RequireBusinessPermission(Permissions.Products.ManageStock);

        group.MapPatch(
            "/{id:guid}/stock/remove",
            RemoveStock)
            .RequireBusinessPermission(Permissions.Products.ManageStock);

        group.MapPost(
            "/",
            CreateProduct)
            .RequireBusinessPermission(Permissions.Products.Create);

        group.MapDelete(
            "/{id:guid}",
            DeleteProduct)
            .RequireBusinessPermission(Permissions.Products.Delete);

        group.MapPost(
    "/{id:guid}/variants",
    AddProductVariant)
    .RequireBusinessPermission(Permissions.Products.Update);

        group.MapPost(
    "/{id:guid}/option-groups",
    AddProductOptionGroup)
    .RequireBusinessPermission(Permissions.Products.Update);

        group.MapGet(
            "/",
            GetProducts)
            .AllowAnonymous();

        group.MapGet(
            "/{id:guid}",
            GetProductById)
            .AllowAnonymous();

        group.MapPut(
            "/{id:guid}",
            UpdateProduct)
            .RequireBusinessPermission(Permissions.Products.Update);

       

        return app;
    }

    private static async Task<IResult> ActivateProduct(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ActivateProductCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return ApiResults.Failure(result.Error);

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateProduct(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeactivateProductCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return ApiResults.Failure(result.Error);

        return Results.NoContent();
    }

    private static async Task<IResult> AddProductOption(
        Guid id,
        Guid optionGroupId,
        [FromBody] AddProductOptionRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddProductOptionCommand(
            id,
            optionGroupId,
            request.Name,
            request.AdditionalPrice,
            request.DisplayOrder);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Created(
            $"/api/products/{id}",
            new { id = result.Value });
    }

    public sealed record AddProductOptionRequest(
        string Name,
        decimal AdditionalPrice,
        int DisplayOrder);


    private static async Task<IResult> AddStock(
     Guid id,
     HttpContext httpContext,
     [FromBody] StockRequest request,
     [FromServices] ISender sender,
     CancellationToken cancellationToken)
    {
        var businessId = httpContext.Request.Headers["X-Business-Id"]
    .FirstOrDefault();

        if (!Guid.TryParse(businessId, out var businessGuid))
        {
            return ApiResults.Failure(
                new eMarket.SharedKernel.Results.Error(
                    "Business.Context.Required",
                    "A valid X-Business-Id header is required."));
        }
        var result = await sender.Send(
            new AddStockCommand(
    id,
    businessGuid,
    request.Quantity),
            cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveStock(
        Guid id,
        HttpContext httpContext,
        [FromBody] StockRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var businessId = httpContext.Request.Headers["X-Business-Id"]
    .FirstOrDefault();

        if (!Guid.TryParse(businessId, out var businessGuid))
        {
            return ApiResults.Failure(
                new eMarket.SharedKernel.Results.Error(
                    "Business.Context.Required",
                    "A valid X-Business-Id header is required."));
        }
        var result = await sender.Send(
            new RemoveStockCommand(
    id,
    businessGuid,
    request.Quantity),
            cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.NoContent();
    }

    private static async Task<IResult> AddProductVariant(
        Guid id,
        [FromBody] AddProductVariantRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddProductVariantCommand(
            id,
            request.Name,
            request.Sku,
            request.Price,
            request.StockQuantity);

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Created(
            $"/api/products/{id}",
            new { id = result.Value });
    }

    private static async Task<IResult> AddProductOptionGroup(
        Guid id,
        [FromBody] AddProductOptionGroupRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddProductOptionGroupCommand(
            id,
            request.Name,
            request.IsRequired,
            request.MinSelections,
            request.MaxSelections,
            request.DisplayOrder);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Created(
            $"/api/products/{id}",
            new { id = result.Value });
    }

    public sealed record AddProductOptionGroupRequest(
        string Name,
        bool IsRequired,
        int MinSelections,
        int MaxSelections,
        int DisplayOrder);


    public sealed record AddProductVariantRequest(
        string Name,
        string? Sku,
        decimal Price,
        int StockQuantity);

    private static async Task<IResult> CreateProduct(
        [FromBody] CreateProductCommand command,
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

        return Results.Created(
            $"/api/products/{result.Value.Id}",
            result.Value);
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return ApiResults.Failure(
                new eMarket.SharedKernel.Results.Error(
                    "Product.RouteId.Mismatch",
                    "Route id and request id must match."));
        }

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }

    

    private static async Task<IResult> GetProducts(
        [AsParameters] GetProductsQuery query,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductByIdQuery(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeleteProduct(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteProductCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return ApiResults.Failure(result.Error);

        return Results.NoContent();
    }
}
