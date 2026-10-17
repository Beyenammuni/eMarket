using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Identity.Entities;

public sealed class UserAddress : Entity<Guid>
{
    private UserAddress()
    {
    }

    private UserAddress(
        Guid id,
        UserId userId,
        string title,
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
        decimal? latitude,
        decimal? longitude,
        bool isDefault)
    {
        Id = id;
        UserId = userId;
        Title = title;
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
        IsDefault = isDefault;
    }

    public UserId UserId { get; private set; } = default!;

    public string Title { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string AddressLine { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string District { get; private set; } = default!;
    public string PostalCode { get; private set; } = default!;
    public string Neighborhood { get; private set; } = default!;
    public string Street { get; private set; } = default!;
    public string BuildingNumber { get; private set; } = default!;
    public string ApartmentNumber { get; private set; } = default!;

    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    public bool IsDefault { get; private set; }

    public static UserAddress Create(
        UserId userId,
        string title,
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
        decimal? latitude,
        decimal? longitude,
        bool isDefault)
    {
        return new UserAddress(
            Guid.NewGuid(),
            userId,
            title.Trim(),
            fullName.Trim(),
            phoneNumber.Trim(),
            addressLine.Trim(),
            city.Trim(),
            district.Trim(),
            postalCode.Trim(),
            neighborhood.Trim(),
            street.Trim(),
            buildingNumber.Trim(),
            apartmentNumber.Trim(),
            latitude,
            longitude,
            isDefault);
    }

    public void Update(
        string title,
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
        decimal? latitude,
        decimal? longitude)
    {
        Title = title.Trim();
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        AddressLine = addressLine.Trim();
        City = city.Trim();
        District = district.Trim();
        PostalCode = postalCode.Trim();
        Neighborhood = neighborhood.Trim();
        Street = street.Trim();
        BuildingNumber = buildingNumber.Trim();
        ApartmentNumber = apartmentNumber.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}
