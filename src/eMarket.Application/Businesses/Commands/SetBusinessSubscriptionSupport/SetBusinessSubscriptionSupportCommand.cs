using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.SetBusinessSubscriptionSupport;

public sealed record SetBusinessSubscriptionSupportCommand(
    Guid BusinessId,
    bool Enabled
) : IRequest<Result<bool>>;
