using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Payments;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace eMarket.Application.Subscriptions.Commands.CreateSubscriptionPayment;

internal sealed class CreateSubscriptionPaymentCommandHandler
    : IRequestHandler<
        CreateSubscriptionPaymentCommand,
        Result<CreateSubscriptionPaymentResponse>>
{
    private readonly ISubscriptionDbContext _subscriptionContext;
    private readonly ICatalogDbContext _catalog;
    private readonly IBusinessRepository _businessRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionPaymentGateway _subscriptionPaymentGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IConfiguration _configuration;

    public CreateSubscriptionPaymentCommandHandler(
        ISubscriptionDbContext subscriptionContext,
        ICatalogDbContext catalog,
        IBusinessRepository businessRepository,
        IPaymentRepository paymentRepository,
        ISubscriptionPaymentGateway subscriptionPaymentGateway,
        
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IConfiguration configuration)
    {
        _subscriptionContext = subscriptionContext;
        _catalog = catalog;
        _businessRepository = businessRepository;
        _paymentRepository = paymentRepository;
        _subscriptionPaymentGateway = subscriptionPaymentGateway;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _configuration = configuration;
    }

    public async Task<Result<CreateSubscriptionPaymentResponse>> Handle(
        CreateSubscriptionPaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is null)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                new Error(
                    "Auth.Unauthorized",
                    "User is not authenticated."));
        }

        // Validate ReturnUrl
        if (!Uri.TryCreate(
                request.ReturnUrl,
                UriKind.Absolute,
                out var returnUri) ||
            (returnUri.Scheme != Uri.UriSchemeHttps &&
             returnUri.Scheme != Uri.UriSchemeHttp))
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                new Error(
                    "Payment.InvalidReturnUrl",
                    "Return URL must be an absolute HTTP(S) URL."));
        }

        var allowedHosts =
            _configuration
                .GetSection("Payments:AllowedReturnHosts")
                .Get<string[]>() ??
            Array.Empty<string>();

        if (allowedHosts.Length > 0 &&
            !allowedHosts.Contains(
                returnUri.Host,
                StringComparer.OrdinalIgnoreCase))
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                new Error(
                    "Payment.InvalidReturnUrl",
                    "Return URL host is not allowed."));
        }

        var subscriptionId =
            SubscriptionId.Create(
                request.SubscriptionId);

        var subscription =
            await _subscriptionContext.Subscriptions
                .Include(x => x.Items)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == subscriptionId &&
                        x.UserId == _currentUser.UserId,
                    cancellationToken);

        if (subscription is null)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                SubscriptionErrors.NotFound);
        }

        if (subscription.Status == SubscriptionStatus.Cancelled)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                new Error(
                    "Subscription.Cancelled",
                    "The subscription has been cancelled."));
        }

        if (subscription.Items.Count == 0)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                SubscriptionErrors.EmptyBasket);
        }

        // Get products belonging to the subscription business.
        var productIds = subscription.Items
            .Select(x => x.ProductId)
            .ToList();

        var products = await _catalog.Products
            .Where(x =>
                productIds.Contains(x.Id) &&
                x.BusinessId == subscription.BusinessId &&
                x.Status == ProductStatus.Active)
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                SubscriptionErrors.DifferentBusinesses);
        }

        // Calculate weekly basket value.
        decimal weeklyAmount = 0m;

        foreach (var subscriptionItem in subscription.Items)
        {
            var product = products.First(
                x => x.Id == subscriptionItem.ProductId);

            if (product.Price.Amount <= 0)
            {
                return Result<CreateSubscriptionPaymentResponse>.Failure(
                    new Error(
                        "Subscription.InvalidProductPrice",
                        $"Product {product.Id.Value} has an invalid price."));
            }

            weeklyAmount +=
                product.Price.Amount *
                subscriptionItem.Quantity;
        }

        // Monthly price:
        // weekly basket × 52 weeks ÷ 12 months.
        var monthlyAmount =
            Math.Round(
                weeklyAmount * 52m / 12m,
                2,
                MidpointRounding.AwayFromZero);

        if (monthlyAmount <= 0)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                PaymentErrors.InvalidAmount);
        }

        var business =
            await _businessRepository.GetByIdAsync(
                subscription.BusinessId,
                cancellationToken);

        if (business is null)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                new Error(
                    "Payment.BusinessNotFound",
                    "The business associated with the subscription was not found."));
        }

        //if (string.IsNullOrWhiteSpace(
        //        business.IyzicoSubMerchantKey))
        //{
        //    return Result<CreateSubscriptionPaymentResponse>.Failure(
        //        new Error(
        //            "Payment.SubMerchantNotConfigured",
        //            "The business is not configured for marketplace payments."));
        //}

        // Prevent duplicate pending/succeeded subscription payments.
        var existingPayment =
            await _paymentRepository.GetBySubscriptionIdAsync(
                subscription.Id.Value,
                cancellationToken);

        if (existingPayment is not null &&
            existingPayment.Status != PaymentStatus.Failed)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                PaymentErrors.AlreadyExists);
        }

        var currency = products
            .Select(x => x.Price.Currency)
            .First();

        var paymentResult = Payment.Create(
            null,
            subscription.Id.Value,
            eMarket.Domain.Catalog.Products.ValueObjects.Money.Create(
                monthlyAmount,
                currency),
            PaymentMethod.Card,
            business.PlatformCommissionRate);

        if (paymentResult.IsFailure)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                paymentResult.Error);
        }

        var payment = paymentResult.Value!;

        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);

        var fullName =
            subscription.DeliveryAddress.FullName.Trim();

        var nameParts = fullName.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        var buyerName =
            nameParts.Length > 1
                ? string.Join(
                    ' ',
                    nameParts.Take(nameParts.Length - 1))
                : nameParts[0];

        var buyerSurname =
            nameParts.Length > 1
                ? nameParts[^1]
                : nameParts[0];

        var buyer = new PaymentBuyer(
            _currentUser.UserId.Value,
            buyerName,
            buyerSurname,
            _currentUser.Email ?? string.Empty,
            subscription.DeliveryAddress.PhoneNumber);

        var merchant = new PaymentGatewayMerchant(
        business.IyzicoSubMerchantKey,
        business.PlatformCommissionRate);

        var deliveryAddress =
            new PaymentGatewayAddress(
                subscription.DeliveryAddress.FullName,
                subscription.DeliveryAddress.PhoneNumber,
                subscription.DeliveryAddress.AddressLine,
                subscription.DeliveryAddress.City,
                subscription.DeliveryAddress.District,
                subscription.DeliveryAddress.PostalCode,
                subscription.DeliveryAddress.Neighborhood,
                subscription.DeliveryAddress.Street,
                subscription.DeliveryAddress.BuildingNumber,
                subscription.DeliveryAddress.ApartmentNumber,
                subscription.DeliveryAddress.Latitude,
                subscription.DeliveryAddress.Longitude);

        var gatewayItems = subscription.Items
            .Select(subscriptionItem =>
            {
                var product = products.First(
                    x => x.Id == subscriptionItem.ProductId);

                return new PaymentGatewayItem(
                    product.Id.Value,
                    product.Name.Value,
                    product.Price.Amount,
                    subscriptionItem.Quantity);
            })
            .ToList();

        var gatewayResult =
    await _subscriptionPaymentGateway.CreateCheckoutFormAsync(
        new SubscriptionPaymentGatewayRequest(
            payment.Id.Value,
            payment.Amount.Amount,
            payment.Amount.Currency.ToString(),
            request.ReturnUrl,
            new SubscriptionPaymentBuyer(
                buyer.Name,
                buyer.Surname,
                buyer.Email,
                buyer.Phone,
                string.Empty),
            new SubscriptionPaymentAddress(
                deliveryAddress.FullName,
                deliveryAddress.PhoneNumber,
                deliveryAddress.AddressLine,
                deliveryAddress.City,
                deliveryAddress.District,
                deliveryAddress.PostalCode,
                deliveryAddress.Neighborhood,
                deliveryAddress.Street,
                deliveryAddress.BuildingNumber,
                deliveryAddress.ApartmentNumber)),
        cancellationToken);

        if (!gatewayResult.IsSuccess)
        {
            return Result<CreateSubscriptionPaymentResponse>.Failure(
                new Error(
                    "Payment.ProviderFailed",
                    gatewayResult.Error ??
                    "Payment provider failed."));
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<CreateSubscriptionPaymentResponse>.Success(
     new CreateSubscriptionPaymentResponse(
         payment.Id.Value,
         subscription.Id.Value,
         payment.Amount.Amount,
         payment.Amount.Currency.ToString(),
         gatewayResult.CheckoutFormContent!));
    }
}
