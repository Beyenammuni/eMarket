namespace eMarket.Application.Identity.Auth.Commands.Login;

public sealed record LoginResponse(
    Guid UserId,
    string Email,
    string AccessToken);
