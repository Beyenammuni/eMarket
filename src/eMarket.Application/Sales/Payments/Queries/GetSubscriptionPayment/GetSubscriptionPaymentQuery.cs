using eMarket.SharedKernel.Results;
using MediatR;



namespace eMarket.Application.Sales.Payments.Queries.GetSubscriptionPayment;

public sealed record GetSubscriptionPaymentQuery(
    Guid SubscriptionId)
    : IRequest<Result<GetSubscriptionPaymentResponse>>;
