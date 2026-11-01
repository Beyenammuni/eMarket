
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.CompleteSellerPlanCheckout;

public sealed record CompleteSellerPlanCheckoutCommand(
    Guid PaymentId,
    string Token
) : IRequest<Result>;
