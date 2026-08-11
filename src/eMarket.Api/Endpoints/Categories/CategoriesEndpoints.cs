using eMarket.Api.Features.Categories;
using eMarket.Application.Catalog.Categories.Commands.ActivateCategory;
using eMarket.Application.Catalog.Categories.Commands.CreateCategory;
using eMarket.Application.Catalog.Categories.Commands.DeactivateCategory;
using eMarket.Application.Catalog.Categories.Commands.UpdateCategory;
using eMarket.Application.Catalog.Categories.Queries.GetCategoryById;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eMarket.Api.Endpoints.Categories;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapPost("/", CreateCategory);
        group.MapGet("/{id:guid}", GetCategoryById);
        group.MapGet("/", GetCategories);
        group.MapPut("/{id:guid}", UpdateCategory);
        group.MapPatch("/{id:guid}/deactivate", DeactivateCategory);
        group.MapPatch("/{id:guid}/Activate",ActivateCategory);

        return app;
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
            Results.BadRequest);
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
            Results.BadRequest);
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

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            Results.Ok,
            error => Results.BadRequest(error));
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
            Results.BadRequest);
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
            error => Results.NotFound(error));
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
            error => Results.BadRequest(error));
    }

}
