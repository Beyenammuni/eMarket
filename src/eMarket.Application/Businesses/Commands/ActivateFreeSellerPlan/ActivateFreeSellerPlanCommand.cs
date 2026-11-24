using MediatR;

namespace eMarket.Application.Businesses.Commands.ActivateFreeSellerPlan;

public sealed record ActivateFreeSellerPlanCommand(
    Guid BusinessId,
    Guid SellerPlanId
) : IRequest<Guid>;
