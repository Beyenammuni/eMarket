using eMarket.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace eMarket.Infrastructure.Security;

public sealed class DataProtectionCustomerIdentityProtector
    : ICustomerIdentityProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionCustomerIdentityProtector(
        IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(
            "Pazora.CustomerProfile.IdentityNumber.v1");
    }

    public string Protect(string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        return _protector.Protect(plaintext);
    }

    public string Unprotect(string protectedData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedData);
        return _protector.Unprotect(protectedData);
    }
}
