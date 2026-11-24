
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
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
                new Error(
                    "Auth.Unauthorized",
                    "User is not authenticated."));
        }

        var identityNumber = request.IdentityNumber?.Trim();

        if (!IsValidTurkishIdentityNumber(identityNumber))
        {
            return Result.Failure(
                new Error(
                    "User.InvalidIdentityNumber",
                    "Turkish identity number is invalid."));
        }

        var user = await _userRepository.GetByIdAsync(
            _currentUser.UserId,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure(
                new Error(
                    "User.NotFound",
                    "User was not found."));
        }

        var encryptedIdentityNumber =
            _identityProtector.Protect(identityNumber!);

        var updateResult =
            user.SetEncryptedIdentityNumber(encryptedIdentityNumber);

        if (updateResult.IsFailure)
            return updateResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsValidTurkishIdentityNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length != 11 ||
            value[0] == '0' ||
            !value.All(c => c is >= '0' and <= '9'))
        {
            return false;
        }

        var digits = value.Select(c => c - '0').ToArray();

        var oddSum =
            digits[0] + digits[2] + digits[4] +
            digits[6] + digits[8];

        var evenSum =
            digits[1] + digits[3] +
            digits[5] + digits[7];

        var tenthDigit = ((oddSum * 7) - evenSum) % 10;

        if (tenthDigit < 0)
            tenthDigit += 10;

        var eleventhDigit = digits.Take(10).Sum() % 10;

        return digits[9] == tenthDigit &&
               digits[10] == eleventhDigit;
    }
}
