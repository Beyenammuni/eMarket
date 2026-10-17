namespace eMarket.Application.Businesses.Queries.GetBusinessById;

public sealed record GetBusinessByIdResponse(
    Guid Id,
    string Name,
    int TypeId,
    string TypeName,
    string Status,
    bool SupportsSubscriptions,
    DateTime CreatedAt,
    string Email,
    string PhoneNumber,
    string Address,
    string City,
    string Country,
    string LegalName,
string? IdentityNumber,
string? TaxNumber,
string? TaxOffice);
