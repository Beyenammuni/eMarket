using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Identity.Entities;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Auth.Commands;

public sealed record CreateUserAddressCommand(
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
    bool IsDefault)
    : IRequest<Result>;

public sealed class CreateUserAddressCommandHandler
    : IRequestHandler<CreateUserAddressCommand, Result>
{
    private readonly IUserAddressRepository _addressRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    public CreateUserAddressCommandHandler(
        IUserAddressRepository addressRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CreateUserAddressCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result.Failure(
                new Error(
                    "User.NotAuthenticated",
                    "User is not authenticated."));
        }

        var address = UserAddress.Create(
            userId,
            request.Title,
            request.FullName,
            request.PhoneNumber,
            request.AddressLine,
            request.City,
            request.District,
            request.PostalCode,
            request.Neighborhood,
            request.Street,
            request.BuildingNumber,
            request.ApartmentNumber,
            request.Latitude,
            request.Longitude,
            request.IsDefault);

        await _addressRepository.AddAsync(
            address,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
