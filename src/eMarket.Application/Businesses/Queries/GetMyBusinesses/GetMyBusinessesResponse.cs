namespace eMarket.Application.Businesses.Queries.GetMyBusinesses;

public sealed record GetMyBusinessesResponse(
    Guid Id,
    string Name,
    int TypeId,
    string TypeName,
    string Status,
    DateTime CreatedAt);
