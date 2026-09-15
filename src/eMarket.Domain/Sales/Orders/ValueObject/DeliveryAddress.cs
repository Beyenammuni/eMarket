using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Sales.Orders.ValueObjects;

public sealed class DeliveryAddress
{
    private DeliveryAddress()
    {
    }

    private DeliveryAddress(
        string fullName,
        string phoneNumber,
        string addressLine,
        string city,
        string district,
        string postalCode,
        string neighborhood,
        string street,
        string buildingNumber,
        string apartmentNumber,
        decimal latitude,
        decimal longitude)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        AddressLine = addressLine;
        City = city;
        District = district;
        PostalCode = postalCode;
        Neighborhood = neighborhood;
        Street = street;
        BuildingNumber = buildingNumber;
        ApartmentNumber = apartmentNumber;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string FullName { get; private set; } = default!;

    public string PhoneNumber { get; private set; } = default!;

    public string AddressLine { get; private set; } = default!;

    public string City { get; private set; } = default!;

    public string District { get; private set; } = default!;

    public string PostalCode { get; private set; } = default!;

    public decimal Latitude { get; private set; }

    public decimal Longitude { get; private set; }
    public string Neighborhood { get; private set; } = default!;
    public string Street { get; private set; } = default!;
    public string BuildingNumber { get; private set; } = default!;
    public string ApartmentNumber { get; private set; } = default!;

    public static Result<DeliveryAddress> Create(
        string fullName,
        string phoneNumber,
        string addressLine,
        string city,
        string district,
        string postalCode,
        string neighborhood,
        string street,
        string buildingNumber,
        string apartmentNumber,
        decimal latitude,
        decimal longitude)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result<DeliveryAddress>.Failure(new Error("Address.FullName.Required", "Full name is required."));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result<DeliveryAddress>.Failure(new Error("Address.PhoneNumber.Required", "Phone number is required."));

        var address = new DeliveryAddress(
            fullName.Trim(),
            phoneNumber.Trim(),
            addressLine.Trim(),
            city.Trim(),
            district.Trim(),
            postalCode.Trim(),
            neighborhood?.Trim() ?? string.Empty,
            street?.Trim() ?? string.Empty,
            buildingNumber?.Trim() ?? string.Empty,
            apartmentNumber?.Trim() ?? string.Empty,
            latitude,
            longitude);

        return Result<DeliveryAddress>.Success(address);
    }
}
