
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken)
    : IRequest<Result>;
