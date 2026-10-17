
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Identity.Auth.Commands.RefreshToken;

internal sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IIdentityDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public RefreshTokenCommandHandler(
        IIdentityDbContext context,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<RefreshTokenResponse>.Failure(
                UserErrors.InvalidRefreshToken);
        }

        var tokenHash = _refreshTokenService.HashToken(
            request.RefreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        var now = DateTime.UtcNow;

        if (storedToken is null || !storedToken.IsValid(now))
        {
            return Result<RefreshTokenResponse>.Failure(
                UserErrors.InvalidRefreshToken);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.Id == storedToken.UserId,
                cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            return Result<RefreshTokenResponse>.Failure(
                UserErrors.InvalidRefreshToken);
        }

        var roleIds = await _context.UserRoleAssignments
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

        var roles = roleIds
            .Select(id => Enumeration.GetAll<UserRole>()
                .FirstOrDefault(x => x.Id == id)?.Name)
            .Where(x => x is not null)
            .Cast<string>()
            .ToList();

        var accessToken = _jwtService.GenerateToken(
            user.Id,
            user.Email.Value,
            roles);

        var newRefreshToken = _refreshTokenService.GenerateToken();
        var newRefreshTokenHash = _refreshTokenService.HashToken(
            newRefreshToken);

        storedToken.Revoke(now, newRefreshTokenHash);

        _context.RefreshTokens.Add(
    new eMarket.Domain.Identity.Entities.RefreshToken(
                Guid.NewGuid(),
                user.Id,
                newRefreshTokenHash,
                now,
                now.AddDays(7)));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(
                user.Id.Value,
                user.Email.Value,
                accessToken,
                newRefreshToken));
    }
}
