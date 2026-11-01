
using MediatR;

namespace eMarket.Application.Businesses.Commands.UpdateSellerPlan;

public sealed record UpdateSellerPlanCommand(
    Guid Id,
    string Name,
    decimal Price,
    int DurationDays,
    bool IsActive
) : IRequest<bool>;
