using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.SharedKernel.Exceptions;

public sealed class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}
