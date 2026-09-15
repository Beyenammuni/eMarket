namespace eMarket.Application.Identity.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordResponse(string Message, string? ResetToken);
