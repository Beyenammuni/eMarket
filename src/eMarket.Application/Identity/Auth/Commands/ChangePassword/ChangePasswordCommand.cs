using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result>;
