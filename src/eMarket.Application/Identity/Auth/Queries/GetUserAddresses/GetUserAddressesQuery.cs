using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Addresses.Queries;

public sealed record GetUserAddressesQuery
    : IRequest<Result<IReadOnlyList<UserAddressResponse>>>;

public sealed record UserAddressResponse(
    Guid Id,
    string Title,
    string FullName,
    string PhoneNumber,
    string AddressLine,
    string City,
    string District,
    string PostalCode,
    string Neighborhood,
    string Street,
    string BuildingNumber,
    string ApartmentNumber,
    decimal? Latitude,
    decimal? Longitude,
    bool IsDefault);

public sealed class GetUserAddressesQueryHandler
    : IRequestHandler<GetUserAddressesQuery, Result<IReadOnlyList<UserAddressResponse>>>
{
    private readonly IUserAddressRepository _addressRepository;
    private readonly ICurrentUser _currentUser;

    public GetUserAddressesQueryHandler(
        IUserAddressRepository addressRepository,
        ICurrentUser currentUser)
    {
        _addressRepository = addressRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<UserAddressResponse>>> Handle(
        GetUserAddressesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result<IReadOnlyList<UserAddressResponse>>.Failure(
                new Error(
                    "User.NotAuthenticated",
                    "User is not authenticated."));
        }

        var addresses = await _addressRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        var response = addresses
            .Select(x => new UserAddressResponse(
                x.Id,
                x.Title,
                x.FullName,
                x.PhoneNumber,
                x.AddressLine,
                x.City,
                x.District,
                x.PostalCode,
                x.Neighborhood,
                x.Street,
                x.BuildingNumber,
                x.ApartmentNumber,
                x.Latitude,
                x.Longitude,
                x.IsDefault))
            .ToList();

        return Result<IReadOnlyList<UserAddressResponse>>.Success(response);
    }
}
