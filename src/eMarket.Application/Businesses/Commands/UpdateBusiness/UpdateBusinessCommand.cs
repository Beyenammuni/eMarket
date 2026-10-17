using eMarket.SharedKernel.Results;
using MediatR;

public sealed record UpdateBusinessCommand(
    Guid BusinessId,
    string Name,
    int Type,
    string Email,
    string PhoneNumber,
    string Address,
    string City,
    string Country,
    string LegalName,
    string? IdentityNumber,
    string? TaxNumber,
    string? TaxOffice)
    : IRequest<Result<UpdateBusinessResponse>>;
