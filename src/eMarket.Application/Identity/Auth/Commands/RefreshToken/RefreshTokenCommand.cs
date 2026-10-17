
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken)
    : IRequest<Result<RefreshTokenResponse>>;

public sealed record RefreshTokenResponse(
    Guid UserId,
    string Email,
    string AccessToken,
    string RefreshToken);
