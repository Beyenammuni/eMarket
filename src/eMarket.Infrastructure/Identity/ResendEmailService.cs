
using eMarket.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Resend;

namespace eMarket.Infrastructure.Identity;

public sealed class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;

    public ResendEmailService(
        IResend resend,
        IConfiguration configuration)
    {
        _resend = resend;
        _configuration = configuration;
    }

    public async Task SendPasswordResetAsync(
        string email,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var from = _configuration["Resend:FromEmail"]
            ?? "onboarding@resend.dev";

        var message = new EmailMessage
        {
            From = from,
            To = new[] { email },
            Subject = "Pazora - Şifre Sıfırlama",
            HtmlBody = $"""
                <div style="font-family:Arial,sans-serif;max-width:520px;margin:auto">
                    <h2>Pazora</h2>
                    <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın.</p>
                    <p>
                        <a href="{System.Net.WebUtility.HtmlEncode(resetUrl)}"
                           style="display:inline-block;padding:12px 20px;background:#16a34a;color:white;text-decoration:none">
                            Şifremi Sıfırla
                        </a>
                    </p>
                    <p>Bu bağlantı 30 dakika içinde sona erer.</p>
                    <p>Bu isteği siz yapmadıysanız bu e-postayı dikkate almayın.</p>
                </div>
                """
        };

        await _resend.EmailSendAsync(message, cancellationToken);
    }
}
