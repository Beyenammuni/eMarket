using eMarket.Api.Features.Categories;
using eMarket.Application.Catalog.Categories.Commands.ActivateCategory;
using eMarket.Application.Catalog.Categories.Commands.CreateCategory;
using eMarket.Application.Catalog.Categories.Commands.DeactivateCategory;
using eMarket.Application.Catalog.Categories.Commands.UpdateCategory;
using eMarket.Application.Catalog.Categories.Queries.GetCategoryById;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.SharedKernel.Constants;

namespace eMarket.Api.Endpoints.Catalog.Categories;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapPost("/", CreateCategory)
            .RequireRoles(Roles.Admin, Roles.SuperAdmin);
        group.MapGet("/{id:guid}", GetCategoryById);
        group.MapGet("/", GetCategories);
        group.MapPut("/{id:guid}", UpdateCategory)
            .RequireRoles(Roles.Admin, Roles.SuperAdmin);
        group.MapPatch("/{id:guid}/deactivate", DeactivateCategory)
            .RequireRoles(Roles.Admin, Roles.SuperAdmin);
        group.MapPatch("/{id:guid}/activate",ActivateCategory)
            .RequireRoles(Roles.Admin, Roles.SuperAdmin);

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
            ApiResults.Failure);
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

}
