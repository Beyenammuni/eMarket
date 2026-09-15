using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.SharedKernel.Rules;

public interface IBusinessRule
{
    bool IsBroken();

    string Message { get; }
}
