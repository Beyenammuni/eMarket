
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
        string iban,
        string? representativeFullName,
        string? representativeIdentityNumber)
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
        RepresentativeFullName = representativeFullName;
        RepresentativeIdentityNumber = representativeIdentityNumber;
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

    public string? RepresentativeFullName { get; private set; }
    public string? RepresentativeIdentityNumber { get; private set; }

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
        string iban,
        string? representativeFullName = null,
        string? representativeIdentityNumber = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            return Failure("InvalidLegalName", "Legal name is required.");

        if (string.IsNullOrWhiteSpace(email))
            return Failure("InvalidEmail", "Email is required.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Failure("InvalidPhoneNumber", "Phone number is required.");

        if (string.IsNullOrWhiteSpace(address))
            return Failure("InvalidAddress", "Address is required.");

        if (string.IsNullOrWhiteSpace(city))
            return Failure("InvalidCity", "City is required.");

        if (string.IsNullOrWhiteSpace(country))
            return Failure("InvalidCountry", "Country is required.");

        if (string.IsNullOrWhiteSpace(iban))
            return Failure("InvalidIban", "IBAN is required.");

        if (merchantType == MerchantType.Personal &&
            string.IsNullOrWhiteSpace(identityNumber))
        {
            return Failure(
                "IdentityNumberRequired",
                "Identity number is required for personal merchants.");
        }

        if (merchantType != MerchantType.Personal &&
            (string.IsNullOrWhiteSpace(taxNumber) ||
             string.IsNullOrWhiteSpace(taxOffice)))
        {
            return Failure(
                "TaxInformationRequired",
                "Tax number and tax office are required for company merchants.");
        }

        if (merchantType != MerchantType.Personal &&
            (!string.IsNullOrWhiteSpace(representativeFullName) !=
             !string.IsNullOrWhiteSpace(representativeIdentityNumber)))
        {
            return Failure(
                "RepresentativeInformationIncomplete",
                "Representative full name and identity number must be provided together.");
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
                NormalizeIban(iban),
                Normalize(representativeFullName),
                Normalize(representativeIdentityNumber)));
    }

    public Result<BusinessMerchantDetails> UpdateContactDetails(
        string email,
        string phoneNumber,
        string address,
        string city,
        string country)
    {
        return Create(
            MerchantType,
            LegalName,
            IdentityNumber,
            TaxNumber,
            TaxOffice,
            email,
            phoneNumber,
            address,
            city,
            country,
            Iban,
            RepresentativeFullName,
            RepresentativeIdentityNumber);
    }

    public Result<BusinessMerchantDetails> UpdateLegalDetails(
        string legalName,
        string? identityNumber,
        string? taxNumber,
        string? taxOffice,
        string? representativeFullName = null,
        string? representativeIdentityNumber = null)
    {
        return Create(
            MerchantType,
            legalName,
            identityNumber,
            taxNumber,
            taxOffice,
            Email,
            PhoneNumber,
            Address,
            City,
            Country,
            Iban,
            representativeFullName ?? RepresentativeFullName,
            representativeIdentityNumber ?? RepresentativeIdentityNumber);
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
        yield return RepresentativeFullName;
        yield return RepresentativeIdentityNumber;
    }

    private static Result<BusinessMerchantDetails> Failure(
        string code,
        string message)
    {
        return Result<BusinessMerchantDetails>.Failure(
            new Error($"BusinessMerchantDetails.{code}", message));
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeIban(string iban)
        => iban.Trim().Replace(" ", string.Empty).ToUpperInvariant();
}
