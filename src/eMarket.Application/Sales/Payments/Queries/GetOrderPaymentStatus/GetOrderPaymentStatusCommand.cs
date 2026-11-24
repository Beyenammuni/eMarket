using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Sales.Payments.Queries.GetOrderPaymentStatus;

public sealed record GetOrderPaymentStatusQuery(
    Guid OrderId) : IRequest<Result<OrderPaymentStatusResponse>>;

public sealed record OrderPaymentStatusResponse(
    Guid OrderId,
    string PaymentStatus,
    string OrderStatus);
