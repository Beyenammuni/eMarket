namespace eMarket.Application.Catalog.Categories.Queries.GetCategory;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedAt);
