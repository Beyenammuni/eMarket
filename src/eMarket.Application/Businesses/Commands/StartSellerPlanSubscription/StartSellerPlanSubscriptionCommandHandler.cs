
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace eMarket.Application.Businesses.Commands.StartSellerPlanSubscription;

public sealed class StartSellerPlanSubscriptionCommandHandler
    : IRequestHandler<StartSellerPlanSubscriptionCommand, StartSellerPlanSubscriptionResult>
{
    private readonly IBusinessDbContext _businessDb;
    private readonly IIdentityDbContext _identityDb;
    private readonly ICurrentUser _currentUser;
    private readonly ISellerPlanPaymentGateway _paymentGateway;
    private readonly ISensitiveDataProtector _dataProtector;
    private readonly IConfiguration _configuration;

    public StartSellerPlanSubscriptionCommandHandler(
        IBusinessDbContext businessDb,
        IIdentityDbContext identityDb,
        ICurrentUser currentUser,
        ISellerPlanPaymentGateway paymentGateway,
        ISensitiveDataProtector dataProtector,
        IConfiguration configuration)
    {
        _businessDb = businessDb;
        _identityDb = identityDb;
        _currentUser = currentUser;
        _paymentGateway = paymentGateway;
        _dataProtector = dataProtector;
        _configuration = configuration;
    }

    public async Task<StartSellerPlanSubscriptionResult> Handle(
        StartSellerPlanSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate authenticated user
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authentication is required.");

        if (!Uri.TryCreate(request.ReturnUrl, UriKind.Absolute, out var returnUri) ||
    (returnUri.Scheme != Uri.UriSchemeHttps &&
     returnUri.Scheme != Uri.UriSchemeHttp) ||
    !string.IsNullOrEmpty(returnUri.UserInfo))
        {
            throw new InvalidOperationException("Invalid payment return URL.");
        }

        var allowedReturnHosts = _configuration
            .GetSection("Payments:AllowedReturnHosts")
            .Get<string[]>() ?? [];

        var isAllowedHost = allowedReturnHosts.Any(
            host => string.Equals(
                host.Trim(),
                returnUri.Host,
                StringComparison.OrdinalIgnoreCase));

        if (!isAllowedHost)
        {
            throw new InvalidOperationException(
                "Payment return URL host is not allowed.");
        }

        if (returnUri.Scheme != Uri.UriSchemeHttps &&
            !string.Equals(
                returnUri.Host,
                "localhost",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "HTTPS is required for payment return URLs.");
        }

        // 2. Load business
        var businessId = BusinessId.Create(request.BusinessId);

        var business = await _businessDb.Businesses
            .FirstOrDefaultAsync(
                x => x.Id == businessId,
                cancellationToken)
            ?? throw new InvalidOperationException("Business not found.");

        // 3. Only active owner can purchase a plan
        var isOwner = await _businessDb.BusinessMembers
            .AnyAsync(
                x => x.BusinessId == businessId &&
                     x.UserId == userId &&
                     x.IsActive &&
                     x.Role == BusinessRole.Owner,
                cancellationToken);

        if (!isOwner)
            throw new UnauthorizedAccessException(
                "Only the active business owner can purchase a seller plan.");

        // 4. Load active plan
        var plan = await _businessDb.SellerPlans
            .FirstOrDefaultAsync(
                x => x.Id == request.SellerPlanId && x.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException("Seller plan not found or inactive.");

        if (plan.Price <= 0)
            throw new InvalidOperationException(
                "Free plans must be activated through the free-plan activation flow.");

        if (plan.DurationDays <= 0)
            throw new InvalidOperationException("Invalid seller plan duration.");

        // 5. Load buyer account
        var user = await _identityDb.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken)
            ?? throw new InvalidOperationException("User account not found.");

        // 6. Select the correct natural person's identity number.
        // Never use a company's TaxNumber as an identity number.
        var protectedIdentityNumber =
            business.MerchantDetails.MerchantType == MerchantType.Personal
                ? business.MerchantDetails.IdentityNumber
                : business.MerchantDetails.RepresentativeIdentityNumber;

        if (string.IsNullOrWhiteSpace(protectedIdentityNumber))
            throw new InvalidOperationException(
                "The merchant's identity or representative identity number is missing.");

        // Stored identity values must already have been encrypted.
        var identityNumber = _dataProtector.Unprotect(protectedIdentityNumber);

        if (string.IsNullOrWhiteSpace(identityNumber))
            throw new InvalidOperationException("Invalid buyer identity number.");

        // 7. Create pending subscription and payment records first.
        var subscription = SellerPlanSubscription.CreatePending(
            businessId,
            plan.Id);

        var payment = SellerPlanPayment.Create(
            businessId,
            plan.Id,
            subscription.Id,
            plan.Price,
            plan.Currency);

        _businessDb.SellerPlanSubscriptions.Add(subscription);
        _businessDb.SellerPlanPayments.Add(payment);

        await _businessDb.SaveChangesAsync(cancellationToken);

        // 8. Create iyzico checkout
        PaymentGatewayResult gatewayResult;

        var callbackBaseUrl = _configuration["Payments:SellerPlanCallbackUrl"];

        if (string.IsNullOrWhiteSpace(callbackBaseUrl) ||
            !Uri.TryCreate(callbackBaseUrl, UriKind.Absolute, out var callbackUri) ||
            callbackUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Seller plan callback URL must be configured as an HTTPS URL.");
        }

        var callbackUrl = $"{callbackBaseUrl}?paymentId={payment.Id}";

        try
        {
            gatewayResult = await _paymentGateway.CreatePaymentAsync(
                new SellerPlanPaymentRequest(
                    payment.Id,
                    plan.Price,
                    plan.Currency,
request.ReturnUrl,
callbackUrl,
user.Id.Value.ToString(),
                    user.FullName.FirstName,
                    user.FullName.LastName,
                    user.Email.Value,
                    user.PhoneNumber.Value,
                    identityNumber),
                cancellationToken);
        }
        catch
        {
            payment.MarkAsFailed();
            subscription.MarkAsFailed();

            await _businessDb.SaveChangesAsync(cancellationToken);
            throw;
        }

        if (!gatewayResult.IsSuccess ||
            string.IsNullOrWhiteSpace(gatewayResult.PaymentUrl) ||
            string.IsNullOrWhiteSpace(gatewayResult.ProviderPaymentId))
        {
            payment.MarkAsFailed();
            subscription.MarkAsFailed();

            await _businessDb.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException(
                gatewayResult.Error ?? "Could not initialize seller plan payment.");
        }

        // ProviderPaymentId returned here is the checkout token.
        payment.SetProviderToken(gatewayResult.ProviderPaymentId);

        await _businessDb.SaveChangesAsync(cancellationToken);

        return new StartSellerPlanSubscriptionResult(
            subscription.Id,
            payment.Id,
            gatewayResult.PaymentUrl);
    }
}
