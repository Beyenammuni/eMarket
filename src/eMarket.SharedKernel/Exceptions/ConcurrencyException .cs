namespace EMarket.SharedKernel.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException()
        : base("The record was modified by another user.")
    {
    }
}