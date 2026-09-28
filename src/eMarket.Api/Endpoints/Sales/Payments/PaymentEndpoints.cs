using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Sales.Payments.Commands.CreatePayment;
using eMarket.Application.Sales.Payments.Commands.ProcessPaymentWebhook;
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

        group.MapGet("/subscriptions/test-products", async (
    ISubscriptionPaymentGateway gateway,
    CancellationToken cancellationToken) =>
        {
            var result = await gateway.TestListProductsAsync(cancellationToken);

            return Results.Content(
                result,
                "application/json");
        });

        // Payment webhook
        group.MapPost(
            "/webhook",
            ProcessWebhook);

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

    private static async Task<IResult> ProcessWebhook(
        [FromBody] PaymentWebhookRequest request,
        HttpRequest httpRequest,
        IConfiguration configuration,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var configuredSecret =
            configuration["Payments:WebhookSecret"];

        var providedSecret =
            httpRequest.Headers["X-Webhook-Secret"].ToString();

        if (string.IsNullOrWhiteSpace(configuredSecret) ||
            string.IsNullOrWhiteSpace(providedSecret) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(configuredSecret),
                Encoding.UTF8.GetBytes(providedSecret)))
        {
            return ApiResults.Unauthorized();
        }

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
