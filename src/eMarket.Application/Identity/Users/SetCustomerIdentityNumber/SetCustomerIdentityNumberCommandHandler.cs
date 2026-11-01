using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Users.SetCustomerIdentityNumber;

internal sealed class SetCustomerIdentityNumberCommandHandler
    : IRequestHandler<SetCustomerIdentityNumberCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerIdentityProtector _identityProtector;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public SetCustomerIdentityNumberCommandHandler(
        IUserRepository userRepository,
        ICustomerIdentityProtector identityProtector,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _identityProtector = identityProtector;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SetCustomerIdentityNumberCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is null)
        {
            return Result.Failure(
                new Error("Auth.Unauthorized", "User is not authenticated."));
        }

        var identityNumber = request.IdentityNumber?.Trim();

        if (string.IsNullOrWhiteSpace(identityNumber) ||
            identityNumber.Length != 11 ||
            !identityNumber.All(char.IsDigit))
        {
            return Result.Failure(
                new Error(
                    "User.InvalidIdentityNumber",
                    "Turkish identity number must contain 11 digits."));
        }

        var user = await _userRepository.GetByIdAsync(
            _currentUser.UserId,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(
                new Error("User.NotFound", "User was not found."));
        }

        var encryptedIdentityNumber =
            _identityProtector.Protect(identityNumber);

        var updateResult =
            user.SetEncryptedIdentityNumber(encryptedIdentityNumber);

        if (updateResult.IsFailure)
            return updateResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
