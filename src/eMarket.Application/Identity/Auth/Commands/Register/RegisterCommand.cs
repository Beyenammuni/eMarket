using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword)
    : IRequest<Result<RegisterResponse>>;
