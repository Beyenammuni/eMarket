using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Common.Interfaces;

public interface ISensitiveDataProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedData);
}
