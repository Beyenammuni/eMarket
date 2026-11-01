
namespace eMarket.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetAsync(
        string email,
        string resetUrl,
        CancellationToken cancellationToken = default);
}
