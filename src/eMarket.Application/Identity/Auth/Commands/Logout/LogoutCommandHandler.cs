
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Identity.Auth.Commands.Logout;

internal sealed class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, Result>
{
    private readonly IIdentityDbContext _context;
    private readonly IRefreshTokenService _refreshTokenService;

    public LogoutCommandHandler(
        IIdentityDbContext context,
        IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Success();

        var tokenHash = _refreshTokenService.HashToken(
            request.RefreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is not null && storedToken.IsValid(DateTime.UtcNow))
        {
            storedToken.Revoke(DateTime.UtcNow);

            await _context.SaveChangesAsync(cancellationToken);
        }

        // Logout is idempotent: an unknown or already-revoked
        // token does not prevent the client from clearing its cookie.
        return Result.Success();
    }
}
