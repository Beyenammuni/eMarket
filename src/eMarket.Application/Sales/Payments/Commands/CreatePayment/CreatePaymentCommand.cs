using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Payments.Commands.CreatePayment;

public sealed record CreatePaymentCommand(
    Guid OrderId,
    string ReturnUrl)
    : IRequest<Result<CreatePaymentResponse>>;
