using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Payments.Iyzico;

internal sealed class SubMerchantProvisioningWorker
    : BackgroundService
{
    private readonly ISubMerchantProvisioningQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubMerchantProvisioningWorker> _logger;

    public SubMerchantProvisioningWorker(
        ISubMerchantProvisioningQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<SubMerchantProvisioningWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await foreach (var businessId in
            _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(
                    businessId,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while provisioning iyzico sub-merchant for Business {BusinessId}.",
                    businessId);
            }
        }
    }

    private async Task ProcessAsync(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting iyzico sub-merchant provisioning for Business {BusinessId}.",
            businessId);

        using var scope =
            _scopeFactory.CreateScope();

        var subMerchantService =
            scope.ServiceProvider
                .GetRequiredService<IIyzicoSubMerchantService>();

        var businessRepository =
            scope.ServiceProvider
                .GetRequiredService<IBusinessRepository>();

        var unitOfWork =
            scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

        var identityContext =
    scope.ServiceProvider
        .GetRequiredService<IIdentityDbContext>();

        var result =
            await subMerchantService.CreateAsync(
                businessId,
                cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError(
                "iyzico sub-merchant provisioning failed for Business {BusinessId}: {Error}",
                businessId,
                result.Error);

            return;
        }

        if (string.IsNullOrWhiteSpace(
                result.SubMerchantKey))
        {
            _logger.LogError(
                "iyzico returned no SubMerchantKey for Business {BusinessId}.",
                businessId);

            return;
        }

        var business =
    await businessRepository.GetWithMembersAsync(
        Domain.Businesses.BusinessId.Create(businessId),
        cancellationToken);

        if (business is null)
        {
            _logger.LogError(
                "Business {BusinessId} was not found after iyzico provisioning.",
                businessId);

            return;
        }

        if (!string.Equals(
         result.Status,
         "SUCCESS",
         StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(
                "iyzico provisioning did not return SUCCESS for Business {BusinessId}. Status: {Status}",
                businessId,
                result.Status);

            return;
        }

        var setMerchantResult =
            business.SetIyzicoSubMerchant(
                result.SubMerchantKey,
                result.Status);

        if (setMerchantResult.IsFailure)
        {
            _logger.LogError(
                "Failed to save iyzico SubMerchant data for Business {BusinessId}: {Error}",
                businessId,
                setMerchantResult.Error);

            return;
        }

        if (business.IsApproved)
        {
            var activateResult = business.Activate();

            if (activateResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to activate Business {BusinessId}: {Error}",
                    businessId,
                    activateResult.Error);

                return;
            }
        }


        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        if (business.Status == BusinessStatus.Active)
        {
            var owner = business.Members.FirstOrDefault(
                member =>
                    member.IsActive &&
                    member.Role == BusinessRole.Owner);

            if (owner is null)
            {
                _logger.LogError(
                    "No active owner found for Business {BusinessId}.",
                    businessId);

                return;
            }

            var hasSellerRole =
                await identityContext.UserRoleAssignments.AnyAsync(
                    assignment =>
                        assignment.UserId == owner.UserId &&
                        assignment.RoleId == UserRole.Seller.Id,
                    cancellationToken);

            if (!hasSellerRole)
            {
                await identityContext.UserRoleAssignments.AddAsync(
                    new UserRoleAssignment(
                        owner.UserId,
                        UserRole.Seller),
                    cancellationToken);
            }
            if (!hasSellerRole)
            {
                await identityContext.SaveChangesAsync(
                    cancellationToken);
            }
        }

        _logger.LogInformation(
            "iyzico sub-merchant successfully provisioned for Business {BusinessId}.",
            businessId);
    }
}
