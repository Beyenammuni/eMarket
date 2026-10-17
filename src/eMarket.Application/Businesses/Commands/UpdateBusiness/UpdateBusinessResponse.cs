
public sealed record UpdateBusinessResponse(
    Guid Id,
    string Name,
    string Type,
    string LegalName,
    string? IdentityNumber,
    string? TaxNumber,
    string? TaxOffice);
