using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Payments.Commands.ProcessPaymentWebhook;

public sealed record ProcessPaymentWebhookCommand(
    Guid PaymentId,
    string Provider,
    string ProviderPaymentId,
    bool Succeeded)
    : IRequest<Result>;
