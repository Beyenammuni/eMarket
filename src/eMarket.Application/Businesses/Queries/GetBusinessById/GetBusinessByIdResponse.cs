namespace eMarket.Application.Businesses.Queries.GetBusinessById;

public sealed record GetBusinessByIdResponse(
    Guid Id,
    string Name,
    int TypeId,
    string TypeName,
    string Status,
    DateTime CreatedAt);
