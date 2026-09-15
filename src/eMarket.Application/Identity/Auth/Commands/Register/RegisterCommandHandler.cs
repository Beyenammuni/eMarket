using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Identity.Auth.Commands.Register;

internal sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IIdentityDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IIdentityDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var username = request.Username.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(x => x.Email.Value == email.Value, cancellationToken))
            return Result<RegisterResponse>.Failure(
                new Error("User.Email.Exists", "Email is already registered."));

        if (await _context.Users.AnyAsync(x => x.Username == username, cancellationToken))
            return Result<RegisterResponse>.Failure(UserErrors.UsernameExists);

        var userResult = User.Register(
            FullName.Create(request.FirstName, request.LastName),
            email,
            PhoneNumber.Create(request.PhoneNumber),
            username);

        if (userResult.IsFailure)
            return Result<RegisterResponse>.Failure(userResult.Error);

        var user = userResult.Value!;
        var credential = new UserCredential(
            user.Id,
            _passwordHasher.Hash(request.Password));

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.UserCredentials.AddAsync(credential, cancellationToken);
        await _context.UserRoleAssignments.AddAsync(
            new UserRoleAssignment(user.Id, UserRole.Customer),
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RegisterResponse>.Success(
            new RegisterResponse(user.Id.Value, user.Username, user.Email.Value));
    }
}
