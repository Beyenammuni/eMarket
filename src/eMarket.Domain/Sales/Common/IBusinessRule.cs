namespace eMarket.SharedKernel.Common;

public interface IBusinessRule
{
    bool IsBroken();

    string Message { get; }
}
