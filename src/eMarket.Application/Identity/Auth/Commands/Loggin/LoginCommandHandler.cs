using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Common;
using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Identity.Auth.Commands.Login;

internal sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IIdentityDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IIdentityDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {

        Email email;

        try
        {
            email = Email.Create(request.Email);
        }
        catch (ArgumentException)
        {
            return Result<LoginResponse>.Failure(
                UserErrors.InvalidCredentials);
        }



        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.Email.Value == email.Value,
                cancellationToken);

        if (user is null)
        {
            return Result<LoginResponse>.Failure(
                UserErrors.InvalidCredentials);
        }


        if (user.Status != UserStatus.Active)
        {
            return Result<LoginResponse>.Failure(
                UserErrors.AccountNotActive);
        }


        var credential = await _context.UserCredentials
            .FirstOrDefaultAsync(
                x => x.UserId == user.Id,
                cancellationToken);

        if (credential is null)
        {
            return Result<LoginResponse>.Failure(
                UserErrors.InvalidCredentials);
        }


        if (!_passwordHasher.Verify(
                request.Password,
                credential.PasswordHash))
        {
            return Result<LoginResponse>.Failure(
                UserErrors.InvalidCredentials);
        }


        user.RecordLogin();


        var roleIds = await _context.UserRoleAssignments
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

        var roles = roleIds
            .Select(id => Enumeration.GetAll<UserRole>().FirstOrDefault(x => x.Id == id)?.Name)
            .Where(x => x is not null)
            .Cast<string>()
            .ToList();

        var accessToken = _jwtService.GenerateToken(
            user.Id,
            user.Email.Value,
            roles);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result<LoginResponse>.Success(
            new LoginResponse(
                user.Id.Value,
                user.Email.Value,
                accessToken));
    }
}
