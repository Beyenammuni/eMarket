namespace eMarket.Application.Catalog.Categories.Queries.GetCategories;

public sealed record CategoryResponse(Guid Id, string Name,
    string Status, DateTime CreatedAt);
