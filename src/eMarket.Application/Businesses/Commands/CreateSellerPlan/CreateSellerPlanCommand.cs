
using MediatR;

namespace eMarket.Application.Businesses.Commands.CreateSellerPlan;

public sealed record CreateSellerPlanCommand(
    string Code,
    string Name,
    decimal Price,
    int DurationDays,
    string Currency = "TRY"
) : IRequest<Guid>;
