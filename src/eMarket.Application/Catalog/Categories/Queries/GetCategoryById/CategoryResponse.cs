namespace eMarket.Application.Catalog.Categories.Queries.GetCategoryById;

public sealed record CategoryResponse(Guid Id, string Name,
    string Status, DateTime CreatedAt);
