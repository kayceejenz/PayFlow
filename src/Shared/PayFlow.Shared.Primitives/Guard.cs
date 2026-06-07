namespace PayFlow.Shared.Primitives;

public static class Guard
{
    public static void AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
    }

    public static void AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or whitespace.", parameterName);
    }

    public static void AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentException($"{parameterName} cannot be negative.", parameterName);
    }

    public static void AgainstZero(decimal value, string parameterName)
    {
        if (value == 0)
            throw new ArgumentException($"{parameterName} cannot be zero.", parameterName);
    }
}
