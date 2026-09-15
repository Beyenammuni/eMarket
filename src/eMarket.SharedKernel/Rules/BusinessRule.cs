using eMarket.SharedKernel.Exceptions;
using eMarket.SharedKernel.Rules;
using System;
using System.Collections.Generic;
using System.Text;
public static class BusinessRules
{
    public static void Check(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleViolationException(rule.Message);
        }
    }
}
