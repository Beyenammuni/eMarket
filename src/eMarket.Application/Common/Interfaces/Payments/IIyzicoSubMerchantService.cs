namespace eMarket.Application.Common.Interfaces.Payments;

public interface IIyzicoSubMerchantService
{
    Task<SubMerchantProvisioningResult> CreateAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);
}

public sealed record SubMerchantProvisioningResult(
    bool IsSuccess,
    string? SubMerchantKey,
    string? Status,
    string? Error);


