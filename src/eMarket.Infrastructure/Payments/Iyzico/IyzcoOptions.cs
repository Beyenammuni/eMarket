namespace eMarket.Infrastructure.Payments.Iyzico;

public sealed class IyzicoOptions
{
    public const string SectionName = "Payments:Iyzico";

    public string ApiKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;
}
