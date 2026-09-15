using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Identity.Auth.Commands.ChangePassword;

internal sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IIdentityDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IIdentityDbContext context, ICurrentUser currentUser, IPasswordHasher passwordHasher)
    {
        _context = context; _currentUser = currentUser; _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result.Failure(new Error("Auth.Unauthorized", "User is not authenticated."));

        var credential = await _context.UserCredentials.FirstOrDefaultAsync(x => x.UserId == _currentUser.UserId, cancellationToken);
        if (credential is null) return Result.Failure(UserErrors.InvalidCredentials);

        if (!_passwordHasher.Verify(request.CurrentPassword, credential.PasswordHash))
            return Result.Failure(UserErrors.InvalidCredentials);

        if (_passwordHasher.Verify(request.NewPassword, credential.PasswordHash))
            return Result.Failure(UserErrors.SamePassword);

        credential.ChangePassword(_passwordHasher.Hash(request.NewPassword));
        var tokens = await _context.PasswordResetTokens.Where(x => x.UserId == _currentUser.UserId && x.UsedAt == null).ToListAsync(cancellationToken);
        foreach (var token in tokens) token.MarkUsed();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
