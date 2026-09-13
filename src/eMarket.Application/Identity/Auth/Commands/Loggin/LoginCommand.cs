using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password)
    : IRequest<Result<LoginResponse>>;
