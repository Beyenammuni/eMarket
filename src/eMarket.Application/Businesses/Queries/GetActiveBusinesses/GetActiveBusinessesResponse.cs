namespace eMarket.Application.Businesses.Queries.GetActiveBusinesses;

public sealed record GetActiveBusinessesResponse(
    Guid Id,
    string Name,
    int TypeId,
    string TypeName,
    string Status,
    DateTime CreatedAt);
