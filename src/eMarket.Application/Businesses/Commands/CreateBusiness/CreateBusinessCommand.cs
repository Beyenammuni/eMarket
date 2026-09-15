using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.CreateBusiness;

public sealed record CreateBusinessCommand(
    string Name,
    int Type,
    int MerchantType,
    string LegalName,
    string? IdentityNumber,
    string? TaxNumber,
    string? TaxOffice,
    string Email,
    string PhoneNumber,
    string Address,
    string City,
    string Country,
    string Iban)
    : IRequest<Result<CreateBusinessResponse>>;

