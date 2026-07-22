using eMarket.Domain.Identity.Errors;
using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Identity;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<UserRole> _roles = [];

    // 1. Constructor الخاص بـ EF Core
    private User()
    {
    }

    // 2. Constructor الحقيقي
    private User(
        UserId id,
        FullName fullName,
        Email email,
        PhoneNumber phoneNumber)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;

        Status = UserStatus.Pending;
        EmailVerified = false;
        CreatedAt = DateTime.UtcNow;
    }

    // 3. Properties
    public FullName FullName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public PhoneNumber PhoneNumber { get; private set; } = default!;
    public UserStatus Status { get; private set; } = default!;
    public bool EmailVerified { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    // 4. Factory Method
    public static Result<User> Register(
        FullName fullName,
        Email email,
        PhoneNumber phoneNumber)
    {
        var user = new User(UserId.New(), fullName, email, phoneNumber);

        var roleResult = user.AssignRole(UserRole.Customer);

        if (roleResult.IsFailure)
        {
            return Result<User>.Failure(roleResult.Error);
        }

        return Result<User>.Success(user);
    }
    public Result AssignRole(UserRole role)
    {
        if (_roles.Contains(role))
        {
            return Result.Failure(UserErrors.DuplicateRole);
        }

        _roles.Add(role);

        return Result.Success();
    }
    public Result VerifyEmail()
    {
        if (EmailVerified)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        EmailVerified = true;

        // AddDomainEvent(new UserEmailVerifiedDomainEvent(Id));

        return Result.Success();
    }
    public Result ChangeEmail(Email newEmail)
    {
        if (Email == newEmail)
        {
            return Result.Failure(UserErrors.SameEmail);
        }

        Email = newEmail;

        // AddDomainEvent(new UserEmailChangedDomainEvent(Id));

        return Result.Success();
    }
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
    public Result Activate()
    {
        if (Status == UserStatus.Active)
        {
            return Result.Success();
        }

        Status = UserStatus.Active;

        return Result.Success();
    }
    public Result Suspend()
    {
        if (Status == UserStatus.Suspended)
        {
            return Result.Success();
        }

        Status = UserStatus.Suspended;

        return Result.Success();
    }
    // 5. Behaviors
    // AssignRole()
    // RemoveRole()
    // ChangeEmail()
    // VerifyEmail()
}

