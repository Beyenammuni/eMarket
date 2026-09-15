namespace eMarket.Application.Businesses.Commands.SelectBusiness;

public sealed record SelectBusinessResponse(
    Guid BusinessId,
    string BusinessName,
    string BusinessRole,
    string Token);
