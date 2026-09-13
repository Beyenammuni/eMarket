namespace eMarket.Application.Identity.Auth.Commands.Register;

public sealed record RegisterResponse(
    Guid UserId,
    string Username,
    string Email);
