namespace eMarket.Application.Common.Interfaces;

public interface ISellerPlanFeatureService
{
    Task<bool> HasFeatureAsync(
        Guid businessId,
        string featureCode,
        CancellationToken cancellationToken = default);
}
