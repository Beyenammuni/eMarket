using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Businesses.ValueObjects;

public sealed class BusinessMerchantDetails : ValueObject
{
    private BusinessMerchantDetails()
    {
    }

    private BusinessMerchantDetails(
        MerchantType merchantType,
        string legalName,
        string? identityNumber,
        string? taxNumber,
        string? taxOffice,
        string email,
        string phoneNumber,
        string address,
        string city,
        string country,
        string iban)
    {
        MerchantType = merchantType;
        LegalName = legalName;
        IdentityNumber = identityNumber;
        TaxNumber = taxNumber;
        TaxOffice = taxOffice;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        Country = country;
        Iban = iban;
    }

    public MerchantType MerchantType { get; private set; } = default!;

    public string LegalName { get; private set; } = string.Empty;

    public string? IdentityNumber { get; private set; }

    public string? TaxNumber { get; private set; }

    public string? TaxOffice { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string Iban { get; private set; } = string.Empty;

    public static Result<BusinessMerchantDetails> Create(
        MerchantType merchantType,
        string legalName,
        string? identityNumber,
        string? taxNumber,
        string? taxOffice,
        string email,
        string phoneNumber,
        string address,
        string city,
        string country,
        string iban)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.InvalidLegalName",
                    "Legal name is required."));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.InvalidEmail",
                    "Email is required."));
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.InvalidPhoneNumber",
                    "Phone number is required."));
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.InvalidAddress",
                    "Address is required."));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.InvalidCity",
                    "City is required."));
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.InvalidCountry",
                    "Country is required."));
        }

        if (string.IsNullOrWhiteSpace(iban))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.InvalidIban",
                    "IBAN is required."));
        }

        if (merchantType == MerchantType.Personal &&
            string.IsNullOrWhiteSpace(identityNumber))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.IdentityNumberRequired",
                    "Identity number is required for personal merchants."));
        }

        if (merchantType != MerchantType.Personal &&
            (string.IsNullOrWhiteSpace(taxNumber) ||
             string.IsNullOrWhiteSpace(taxOffice)))
        {
            return Result<BusinessMerchantDetails>.Failure(
                new Error(
                    "BusinessMerchantDetails.TaxInformationRequired",
                    "Tax number and tax office are required for company merchants."));
        }

        return Result<BusinessMerchantDetails>.Success(
            new BusinessMerchantDetails(
                merchantType,
                legalName.Trim(),
                Normalize(identityNumber),
                Normalize(taxNumber),
                Normalize(taxOffice),
                email.Trim(),
                phoneNumber.Trim(),
                address.Trim(),
                city.Trim(),
                country.Trim(),
                NormalizeIban(iban)));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MerchantType;
        yield return LegalName;
        yield return IdentityNumber;
        yield return TaxNumber;
        yield return TaxOffice;
        yield return Email;
        yield return PhoneNumber;
        yield return Address;
        yield return City;
        yield return Country;
        yield return Iban;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    private static string NormalizeIban(string iban)
        => iban
            .Trim()
            .Replace(" ", string.Empty)
            .ToUpperInvariant();
}
