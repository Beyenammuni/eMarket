using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Payments.Commands.CompleteOrderCheckout;

public sealed record CompleteOrderCheckoutCommand(
    Guid PaymentId,
    string Token) : IRequest<Result>;
