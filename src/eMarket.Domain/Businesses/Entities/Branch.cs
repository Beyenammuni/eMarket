using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Businesses.Entities;

public sealed class Branch
{
    private Branch()
    {
    }

    private Branch(
        BranchId id,
        BusinessId businessId,
        string name,
        string address,
        string city,
        string district,
        string phoneNumber,
        decimal? latitude,
        decimal? longitude,
        bool isMain)
    {
        Id = id;
        BusinessId = businessId;
        Name = name;
        Address = address;
        City = city;
        District = district;
        PhoneNumber = phoneNumber;
        Latitude = latitude;
        Longitude = longitude;
        IsMain = isMain;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public BranchId Id { get; private set; } = default!;

    public BusinessId BusinessId { get; private set; } = default!;

    public string Name { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string District { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }
    public bool IsMain { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static Result<Branch> Create(
     BusinessId businessId,
     string name,
     string address,
     string city,
     string district,
     string phoneNumber,
     decimal? latitude,
     decimal? longitude,
     bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Branch>.Failure(
                new Error("Branch.Name.Required", "Branch name is required."));

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            return Result<Branch>.Failure(
                new Error("Branch.Latitude.Invalid", "Invalid latitude."));

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            return Result<Branch>.Failure(
                new Error("Branch.Longitude.Invalid", "Invalid longitude."));

        if (latitude.HasValue != longitude.HasValue)
            return Result<Branch>.Failure(
                new Error(
                    "Branch.Coordinates.Incomplete",
                    "Both coordinates must be provided."));

        var branch = new Branch(
            BranchId.New(),
            businessId,
            name.Trim(),
            address?.Trim() ?? string.Empty,
            city?.Trim() ?? string.Empty,
            district?.Trim() ?? string.Empty,
            phoneNumber?.Trim() ?? string.Empty,
            latitude,
            longitude,
            isMain);

        return Result<Branch>.Success(branch);
    }
    public void UpdateLocation(
        string address,
        string city,
        string district,
        decimal? latitude,
        decimal? longitude)
    {
        Address = address.Trim();
        City = city.Trim();
        District = district.Trim();
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContact(string phoneNumber)
    {
        PhoneNumber = phoneNumber.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    public Result UpdateDetails(
    string name,
    string address,
    string city,
    string district,
    string phoneNumber,
    decimal? latitude,
    decimal? longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(
                new Error("Branch.Name.Required", "Branch name is required."));

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            return Result.Failure(
                new Error("Branch.Latitude.Invalid", "Invalid latitude."));

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            return Result.Failure(
                new Error("Branch.Longitude.Invalid", "Invalid longitude."));

        if (latitude.HasValue != longitude.HasValue)
            return Result.Failure(
                new Error(
                    "Branch.Coordinates.Incomplete",
                    "Both coordinates must be provided."));

        Name = name.Trim();
        Address = address?.Trim() ?? string.Empty;
        City = city?.Trim() ?? string.Empty;
        District = district?.Trim() ?? string.Empty;
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
