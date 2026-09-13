namespace eMarket.Application.Businesses.Commands.TransferOwnership;

public sealed record TransferOwnershipResponse(
    Guid BusinessId,
    Guid NewOwnerId);
