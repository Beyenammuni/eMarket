namespace eMarket.Application.Businesses.Queries.GetBusinessBranches;

public sealed record GetBusinessBranchesResponse(
    Guid Id,
    string Name,
    string Address,
    string City,
    string District,
    string PhoneNumber,
    decimal? Latitude,
    decimal? Longitude,
    bool IsMain,
    bool IsActive
);
