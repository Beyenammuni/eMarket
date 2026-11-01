using eMarket.Application.Businesses.Commands.CreateSellerPlan;
using eMarket.Application.Businesses.Commands.UpdateSellerPlan;
using eMarket.Application.Businesses.Queries.GetSellerPlans;
using eMarket.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace eMarket.Api.Endpoints.Admin;

public static class SellerPlanEndpoints
{
    public static IEndpointRouteBuilder MapSellerPlanEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/seller-plans")
            .RequireAuthorization(policy =>
                policy.RequireRole(Roles.SuperAdmin));

        group.MapGet("/", async (
            bool activeOnly,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetSellerPlansQuery(activeOnly),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/", async (
            CreateSellerPlanCommand request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Code) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.Price < 0 ||
                request.DurationDays < 0 ||
                string.IsNullOrWhiteSpace(request.Currency) ||
                request.Currency.Trim().Length != 3)
            {
                return Results.BadRequest(new
                {
                    message = "Invalid seller plan details."
                });
            }

            var id = await sender.Send(request, cancellationToken);

            return Results.Created(
                $"/api/admin/seller-plans/{id}",
                new { id });
        });


        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateSellerPlanCommand request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (id != request.Id ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.Price < 0 ||
                request.DurationDays < 0)
            {
                return Results.BadRequest(new
                {
                    message = "Invalid seller plan details."
                });
            }

            var updated = await sender.Send(
                request,
                cancellationToken);

            return updated
                ? Results.NoContent()
                : Results.NotFound();
        });

        return app;

    }

}
