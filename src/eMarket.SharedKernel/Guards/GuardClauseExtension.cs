namespace eMarket.SharedKernel.Guards;

public static class GuardClauseExtension
{
    public static T Null<T>(
        this IGuardClause _,
        T? input,
        string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(input, parameterName);

        return input;
    }

    public static string NullOrWhiteSpace(
        this IGuardClause _,
        string? input,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException(
                $"{parameterName} cannot be empty.",
                parameterName);

        return input;
    }

    public static decimal NegativeOrZero(
        this IGuardClause _,
        decimal value,
        string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} must be greater than zero.");

        return value;
    }

    public static int NegativeOrZero(
        this IGuardClause _,
        int value,
        string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} must be greater than zero.");

        return value;
    }

    public static Guid Empty(
        this IGuardClause _,
        Guid value,
        string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                $"{parameterName} cannot be empty.",
                parameterName);

        return value;
    }

    public static int OutOfRange(
        this IGuardClause _,
        int value,
        int min,
        int max,
        string parameterName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} must be between {min} and {max}.");

        return value;
    }
}