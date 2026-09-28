using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Api.Features.Categories;
using eMarket.Application.Catalog.Categories.Commands.ActivateCategory;
using eMarket.Application.Catalog.Categories.Commands.CreateCategory;
using eMarket.Application.Catalog.Categories.Commands.DeactiveCategory;
using eMarket.Application.Catalog.Categories.Commands.UpdateCategory;
using eMarket.Application.Catalog.Categories.Queries.GetCategories;
using eMarket.Application.Catalog.Categories.Queries.GetCategoryById;
using eMarket.SharedKernel.Constants;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Api.Endpoints.Catalog.Categories;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapPost("/", CreateCategory)
            .RequireBusinessPermission(
                Permissions.Categories.Create);

        group.MapGet("/", GetCategories)
            .RequireBusinessPermission(
                Permissions.Categories.View);

        group.MapGet("/{id:guid}", GetCategoryById)
            .RequireBusinessPermission(
                Permissions.Categories.View);

        group.MapPut("/{id:guid}", UpdateCategory)
            .RequireBusinessPermission(
                Permissions.Categories.Update);

        group.MapPatch(
                "/{id:guid}/deactivate",
                DeactivateCategory)
            .RequireBusinessPermission(
                Permissions.Categories.Deactivate);

        group.MapPatch(
                "/{id:guid}/activate",
                ActivateCategory)
            .RequireBusinessPermission(
                Permissions.Categories.Activate);

        return app;
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            command,
            cancellationToken);

        return result.Match(
            success => Results.Created(
                $"/api/categories/{success.Id}",
                success),
            error => ApiResults.Failure(error));
    }

    private static async Task<IResult> GetCategories(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCategories(),
            cancellationToken);

        return result.Match(
            Results.Ok,
            ApiResults.Failure);
    }

    private static async Task<IResult> GetCategoryById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCategoryByIdQuery(id),
            cancellationToken);

        return result.Match(
            success => Results.Ok(success),
            error => ApiResults.Failure(error));
    }

    private static async Task<IResult> UpdateCategory(
        Guid id,
        UpdateCategoryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.Match(
            Results.Ok,
            error => ApiResults.Failure(error));
    }

    private static async Task<IResult> DeactivateCategory(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeactivateCategoryCommand(id),
            cancellationToken);

        return result.Match(
            () => Results.NoContent(),
            ApiResults.Failure);
    }

    private static async Task<IResult> ActivateCategory(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ActivateCategoryCommand(id),
            cancellationToken);

        return result.Match(
            () => Results.NoContent(),
            ApiResults.Failure);
    }
}

