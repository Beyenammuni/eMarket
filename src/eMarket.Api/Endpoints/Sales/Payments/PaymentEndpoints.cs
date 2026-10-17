using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Sales.Payments.Commands.CompleteOrderCheckout;
using eMarket.Application.Sales.Payments.Commands.CreatePayment;
using eMarket.Application.Sales.Payments.Commands.ProcessPaymentWebhook;
using eMarket.Application.Sales.Payments.Queries.GetSubscriptionPayment;
using eMarket.Application.Subscriptions.Commands.CompleteSubscriptionCheckout;
using eMarket.Application.Subscriptions.Commands.CreateSubscriptionPayment;
using eMarket.Infrastructure.Payments.Iyzico;
using eMarket.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace eMarket.Api.Endpoints.Payments;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments");

        // Order payment
        group.MapPost(
            "/orders/{orderId:guid}",
            CreatePayment)
            .RequireRoles(Roles.Customer);


        // Subscription monthly payment
        group.MapPost(
            "/subscriptions/{subscriptionId:guid}",
            CreateSubscriptionPayment)
            .RequireRoles(Roles.Customer);

        group.MapGet(
    "/subscriptions/{subscriptionId:guid}",
    GetSubscriptionPayment)
    .RequireRoles(Roles.Customer);

        group.MapGet(
    "/subscriptions/checkout-result",
    GetSubscriptionCheckoutResult)
    .RequireRoles(Roles.Customer);
        group.MapPost(
    "/subscriptions/checkout-callback",
    SubscriptionCheckoutCallback);

        group.MapPost(
    "/orders/checkout-callback",
    OrderCheckoutCallback);

        group.MapGet("/subscriptions/test-products", async (
    ISubscriptionPaymentGateway gateway,
    CancellationToken cancellationToken) =>
        {
            var result = await gateway.TestListProductsAsync(cancellationToken);

            return Results.Content(
                result,
                "application/json");
        });

        group.MapPost(
    "/subscriptions/complete",
    CompleteSubscriptionCheckout)
    .RequireRoles(Roles.Customer);

        // Payment webhook
        group.MapPost(
    "/webhook",
    ProcessWebhook)
    .RequireAuthorization();


        return app;
    }

    private static async Task<IResult> CreatePayment(
        Guid orderId,
        [FromBody] CreatePaymentRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreatePaymentCommand(
                orderId,
                request.ReturnUrl),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }
    private static async Task<IResult> GetSubscriptionCheckoutResult(
    [FromQuery] string token,
    [FromServices] ISubscriptionPaymentGateway gateway,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest(new
            {
                error = "Subscription checkout token is required."
            });
        }

        var result = await gateway.GetCheckoutResultAsync(
            token,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                error = result.Error
            });
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> SubscriptionCheckoutCallback(
     HttpRequest request,
     [FromServices] ISender sender,
     CancellationToken cancellationToken)
    {
        var token =
            request.Form["token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest(
                "Subscription checkout token is required.");
        }

        var paymentIdValue =
            request.Query["paymentId"].FirstOrDefault();

        if (!Guid.TryParse(
                paymentIdValue,
                out var paymentId))
        {
            return Results.BadRequest(
                "Payment ID is required.");
        }

        var result = await sender.Send(
            new CompleteSubscriptionCheckoutCommand(
                paymentId,
                token),
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(
                result.Error);
        }

        return Results.Redirect(
            $"http://localhost:5173/subscription/checkout?token={Uri.EscapeDataString(token)}");
    }
    private static async Task<IResult> CreateSubscriptionPayment(
        Guid subscriptionId,
        [FromBody] CreateSubscriptionPaymentRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateSubscriptionPaymentCommand(
                subscriptionId,
                request.ReturnUrl),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }
    private static async Task<IResult> CompleteSubscriptionCheckout(
    [FromBody] CompleteSubscriptionCheckoutRequest request,
    [FromServices] ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CompleteSubscriptionCheckoutCommand(
                request.PaymentId,
                request.Token),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok();
    }
    private static async Task<IResult> ProcessWebhook(
        [FromBody] PaymentWebhookRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken){

        var result = await sender.Send(
            new ProcessPaymentWebhookCommand(
                request.PaymentId,
                request.Provider,
                request.ProviderPaymentId,
                request.Succeeded),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok();
    }
    private static async Task<IResult> GetSubscriptionPayment(
    Guid subscriptionId,
    [FromServices] ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSubscriptionPaymentQuery(subscriptionId),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResults.Failure(result.Error);
        }

        return Results.Ok(result.Value);
    }
    private static async Task<IResult> OrderCheckoutCallback(
    HttpRequest request,
    [FromServices] ISender sender,
     [FromServices] IConfiguration configuration,
    CancellationToken cancellationToken)
    {
        var token =
            request.Form["token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest(
                "Order checkout token is required.");
        }

        var paymentIdValue =
            request.Query["paymentId"].FirstOrDefault();

        if (!Guid.TryParse(
                paymentIdValue,
                out var paymentId))
        {
            return Results.BadRequest(
                "Payment ID is required.");
        }

        var result = await sender.Send(
            new CompleteOrderCheckoutCommand(
                paymentId,
                token),
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(
                result.Error);
        }

        var returnUrl =
    configuration["Payments:OrderReturnUrl"]
    ?? "http://localhost:5173/customer/payment-result";

        var separator = returnUrl.Contains('?') ? "&" : "?";

        return Results.Redirect(
            $"{returnUrl}{separator}paymentId={paymentId}&status=success");
    }
}


public sealed record CreatePaymentRequest(
    string ReturnUrl);

public sealed record CreateSubscriptionPaymentRequest(
    string ReturnUrl);

public sealed record PaymentWebhookRequest(
    Guid PaymentId,
    string Provider,
    string ProviderPaymentId,
    bool Succeeded);

public sealed record CompleteSubscriptionCheckoutRequest(
    Guid PaymentId,
    string Token);
