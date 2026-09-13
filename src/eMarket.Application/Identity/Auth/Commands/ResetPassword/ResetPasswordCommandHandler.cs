using System.Security.Cryptography;
using System.Text;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Identity.Auth.Commands.ResetPassword;

internal sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IIdentityDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IIdentityDbContext context, IPasswordHasher passwordHasher)
    { _context = context; _passwordHasher = passwordHasher; }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
        var token = await _context.PasswordResetTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (token is null || !token.IsValid(DateTime.UtcNow)) return Result.Failure(UserErrors.InvalidResetToken);

        var credential = await _context.UserCredentials.FirstOrDefaultAsync(x => x.UserId == token.UserId, cancellationToken);
        if (credential is null) return Result.Failure(UserErrors.InvalidResetToken);
        if (_passwordHasher.Verify(request.NewPassword, credential.PasswordHash)) return Result.Failure(UserErrors.SamePassword);

        credential.ChangePassword(_passwordHasher.Hash(request.NewPassword));
        token.MarkUsed();
        var otherTokens = await _context.PasswordResetTokens.Where(x => x.UserId == token.UserId && x.Id != token.Id && x.UsedAt == null).ToListAsync(cancellationToken);
        foreach (var other in otherTokens) other.MarkUsed();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
