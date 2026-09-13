public sealed record PaymentWebhookRequest(
    Guid PaymentId,
    string Provider,
    string ProviderPaymentId,
    bool Succeeded);
