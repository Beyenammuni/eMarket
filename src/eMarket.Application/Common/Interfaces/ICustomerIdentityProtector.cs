namespace eMarket.Application.Common.Interfaces;

public interface ICustomerIdentityProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedData);
}
