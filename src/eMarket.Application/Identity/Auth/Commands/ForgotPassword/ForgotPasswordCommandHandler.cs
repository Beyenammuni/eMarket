using System.Security.Cryptography;
using System.Text;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity.Entities;
using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace eMarket.Application.Identity.Auth.Commands.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    private readonly IIdentityDbContext _context;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(IIdentityDbContext context, IConfiguration configuration)
    { _context = context; _configuration = configuration; }

    public async Task<Result<ForgotPasswordResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.Value == email.Value, cancellationToken);
        const string message = "If the email is registered, a password reset token has been generated.";

        if (user is null)
            return Result<ForgotPasswordResponse>.Success(new(message, null));

        var activeTokens = await _context.PasswordResetTokens.Where(x => x.UserId == user.Id && x.UsedAt == null).ToListAsync(cancellationToken);
        foreach (var token in activeTokens) token.MarkUsed();

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        var resetToken = new PasswordResetToken(Guid.NewGuid(), user.Id, hash, DateTime.UtcNow.AddMinutes(30));
        await _context.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var expose = _configuration.GetValue<bool>("Auth:ExposeResetToken");
        return Result<ForgotPasswordResponse>.Success(new(message, expose ? rawToken : null));
    }
}
