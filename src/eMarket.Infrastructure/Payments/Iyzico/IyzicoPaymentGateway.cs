using eMarket.Application.Common.Interfaces.Payments;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace eMarket.Infrastructure.Payments.Iyzico;

internal sealed class IyzicoPaymentGateway : IPaymentGateway
{
    private readonly IyzicoOptions _options;
    private readonly ILogger<IyzicoPaymentGateway> _logger;
    public IyzicoPaymentGateway(
        IOptions<IyzicoOptions> options,
        ILogger<IyzicoPaymentGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PaymentGatewayResult> CreatePaymentAsync(
     PaymentGatewayRequest request,
     CancellationToken cancellationToken = default)
    {
        var options = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl
        };

        var buyer = new Buyer
        {
            Id = request.Buyer.Id.ToString(),
            Name = request.Buyer.Name,
            Surname = request.Buyer.Surname,
            GsmNumber = NormalizeTurkishPhone(request.Buyer.Phone),
            Email = request.Buyer.Email,
            IdentityNumber = request.Buyer.IdentityNumber,
            RegistrationAddress = request.DeliveryAddress.AddressLine,
            City = request.DeliveryAddress.City,
            Country = "Turkey",
            ZipCode = request.DeliveryAddress.PostalCode
        };

        var shippingAddress = CreateAddress(request.DeliveryAddress);
        var billingAddress = CreateAddress(request.DeliveryAddress);
        var basketItems = CreateBasketItems(request);

        var checkoutRequest = new CreateCheckoutFormInitializeRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = request.PaymentId.ToString(),

            Price = request.Amount.ToString(
                "0.00",
                CultureInfo.InvariantCulture),

            PaidPrice = request.Amount.ToString(
                "0.00",
                CultureInfo.InvariantCulture),

            Currency = request.Currency,

            BasketId = (
                request.OrderId ??
                request.SubscriptionId ??
                request.PaymentId
            ).ToString(),

            PaymentGroup = PaymentGroup.PRODUCT.ToString(),
            CallbackUrl = request.ReturnUrl,
            Buyer = buyer,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            BasketItems = basketItems
        };

        try
        {
            var response = await CheckoutFormInitialize.Create(
                checkoutRequest,
                options);

            var isProviderSuccess = string.Equals(
                response.Status,
                Status.SUCCESS.ToString(),
                StringComparison.OrdinalIgnoreCase);

            if (!isProviderSuccess)
            {
                return new PaymentGatewayResult(
    false,
    null,
    null,
    response.ErrorMessage ??
    "iyzico checkout initialization failed.")
                {
                    CheckoutToken = response.Token,
                    CreationStatus = string.IsNullOrWhiteSpace(response.Token)
        ? PaymentCreationStatus.Failed
        : PaymentCreationStatus.Unknown
                };
            }

            if (string.IsNullOrWhiteSpace(response.Token) ||
                string.IsNullOrWhiteSpace(response.PaymentPageUrl))
            {
                return new PaymentGatewayResult(
    false,
    response.PaymentPageUrl,
    null,
    "iyzico returned an incomplete checkout response.")
                {
                    CheckoutToken = response.Token,
                    CreationStatus = PaymentCreationStatus.Unknown
                };
            }

            return new PaymentGatewayResult(
    true,
    response.PaymentPageUrl,
    null,
    null)
            {
                CheckoutToken = response.Token,
                CreationStatus = PaymentCreationStatus.Succeeded
            };
        }
        catch (Exception ex)
        {
            return new PaymentGatewayResult(
                false,
                null,
                null,
                ex.Message)
            {
                CreationStatus = PaymentCreationStatus.Unknown
            };
        }
    }


    public async Task<PaymentCheckoutResult> GetCheckoutResultAsync(
    string token,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new PaymentCheckoutResult(
                false, null, "iyzico token is required.",
                null, null, null, null, null)
            {
                VerificationStatus = PaymentVerificationStatus.Unknown
            };
        }

        var options = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl
        };

        var request = new RetrieveCheckoutFormRequest
        {
            Token = token
        };

        try
        {
            var response = await CheckoutForm.Retrieve(request, options);

            if (!string.Equals(
                    response.Status,
                    Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return new PaymentCheckoutResult(
                    false,
                    response.PaymentId,
                    response.ErrorMessage ??
                        "iyzico checkout result could not be retrieved.",
                    response.ConversationId,
                    response.BasketId,
                    response.Price,
                    response.PaidPrice,
                    response.Currency)
                {
                    VerificationStatus = PaymentVerificationStatus.Unknown,
                    FraudStatus = response.FraudStatus
                };
            }

            var paymentStatus = response.PaymentStatus?.Trim();
            var fraudStatus = response.FraudStatus;

            var verificationStatus = PaymentVerificationStatus.Unknown;
            var error = response.ErrorMessage;

            if (string.Equals(
                    paymentStatus,
                    "FAILURE",
                    StringComparison.OrdinalIgnoreCase) ||
                fraudStatus == -1)
            {
                verificationStatus = PaymentVerificationStatus.Failed;
                error ??= "iyzico payment was rejected.";
            }
            else if (string.Equals(
                         paymentStatus,
                         "SUCCESS",
                         StringComparison.OrdinalIgnoreCase))
            {
                if (fraudStatus == 1)
                {
                    verificationStatus = PaymentVerificationStatus.Succeeded;
                }
                else if (fraudStatus == 0)
                {
                    verificationStatus = PaymentVerificationStatus.Pending;
                    error ??= "Payment is under iyzico fraud review.";
                }
            }
            else if (string.Equals(
                         paymentStatus,
                         "INIT_THREEDS",
                         StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(
                         paymentStatus,
                         "CALLBACK_THREEDS",
                         StringComparison.OrdinalIgnoreCase))
            {
                verificationStatus = PaymentVerificationStatus.Pending;
            }

            return new PaymentCheckoutResult(
                verificationStatus == PaymentVerificationStatus.Succeeded,
                response.PaymentId,
                error,
                response.ConversationId,
                response.BasketId,
                response.Price,
                response.PaidPrice,
                response.Currency)
            {
                VerificationStatus = verificationStatus,
                FraudStatus = fraudStatus
            };
        }
        catch (Exception)
        {
            return new PaymentCheckoutResult(
                false,
                null,
                "Unable to verify the iyzico checkout result.",
                null, null, null, null, null)
            {
                VerificationStatus = PaymentVerificationStatus.Unknown
            };
        }
    }

    private static List<BasketItem> CreateBasketItems(
        PaymentGatewayRequest request)
    {
        var commissionRate =
            request.Merchant.PlatformCommissionRate;

        var sellerShareRate =
            1m - commissionRate / 100m;

        var sourceItems = request.Items.ToList();

        var items = new List<BasketItem>(
            sourceItems.Count);

        foreach (var item in sourceItems)
        {
            var itemTotal =
                item.Price * item.Quantity;

            var sellerPrice =
                Math.Round(
                    itemTotal * sellerShareRate,
                    2,
                    MidpointRounding.AwayFromZero);

            items.Add(
                new BasketItem
                {
                    Id = item.ProductId.ToString(),
                    Name = item.Name,
                    Category1 = "Products",
                    Category2 = "eMarket",
                    ItemType = BasketItemType.PHYSICAL.ToString(),

                    Price = itemTotal.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture),

                    SubMerchantKey =
                        request.Merchant.SubMerchantKey,

                    SubMerchantPrice =
                        sellerPrice.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture)
                });
        }

        return items;
    }

    private static Address CreateAddress(
        PaymentGatewayAddress address)
    {
        return new Address
        {
            ContactName = address.FullName,
            City = address.City,
            Country = "Turkey",
            ZipCode = address.PostalCode,
            Description = BuildAddressDescription(address)
        };
    }

    private static string BuildAddressDescription(
        PaymentGatewayAddress address)
    {
        var parts = new[]
        {
            address.AddressLine,
            address.Neighborhood,
            address.Street,
            address.BuildingNumber,
            address.ApartmentNumber,
            address.District
        };

        return string.Join(
            ", ",
            parts.Where(x =>
                !string.IsNullOrWhiteSpace(x)));
    }
    private static string NormalizeTurkishPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException(
                "Buyer phone number is required.",
                nameof(phone));
        }

        var digits = new string(
            phone.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("0090"))
        {
            digits = digits[4..];
        }
        else if (digits.StartsWith("90"))
        {
            digits = digits[2..];
        }
        else if (digits.StartsWith("0"))
        {
            digits = digits[1..];
        }

        if (digits.Length != 10)
        {
            throw new ArgumentException(
                "Turkish phone number must contain 10 national digits.",
                nameof(phone));
        }

        return $"+90{digits}";
    }

    public async Task<PaymentCheckoutResult> GetPaymentResultAsync(
        string providerPaymentId,
        string paymentConversationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerPaymentId) ||
            string.IsNullOrWhiteSpace(paymentConversationId))
        {
            return new PaymentCheckoutResult(
                false, null, "Payment identifiers are required.",
                null, null, null, null, null)
            {
                VerificationStatus = PaymentVerificationStatus.Unknown
            };
        }

        var options = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl
        };

        var request = new Iyzipay.Request.RetrievePaymentRequest
        {
            Locale = Iyzipay.Model.Locale.TR.ToString(),
            ConversationId = paymentConversationId,
            PaymentId = providerPaymentId,
            PaymentConversationId = paymentConversationId
        };

        try
        {
            var response = await Iyzipay.Model.Payment.Retrieve(
                request,
                options);

            if (!string.Equals(
                    response.Status,
                    Iyzipay.Model.Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return new PaymentCheckoutResult(
                    false,
                    response.PaymentId,
                    response.ErrorMessage ??
                        "iyzico payment could not be retrieved.",
                    response.ConversationId,
                    response.BasketId,
                    response.Price,
                    response.PaidPrice,
                    response.Currency)
                {
                    VerificationStatus = PaymentVerificationStatus.Unknown,
                    FraudStatus = response.FraudStatus
                };
            }

            var isPaymentSuccessful = string.Equals(
    response.PaymentStatus,
    "SUCCESS",
    StringComparison.OrdinalIgnoreCase);
            if (!string.Equals(
        response.PaymentId,
        providerPaymentId,
        StringComparison.Ordinal))
            {
                return new PaymentCheckoutResult(
                    false,
                    response.PaymentId,
                    "Payment ID verification failed.",
                    response.ConversationId,
                    response.BasketId,
                    response.Price,
                    response.PaidPrice,
                    response.Currency)
                {
                    VerificationStatus = PaymentVerificationStatus.Unknown,
                    FraudStatus = response.FraudStatus
                };
            }
            if (!string.Equals(
        response.ConversationId,
        paymentConversationId,
        StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "iyzico ConversationId mismatch. PaymentId: {PaymentId}",
                    providerPaymentId);

                return new PaymentCheckoutResult(
                    false,
                    response.PaymentId,
                    "Payment conversation verification failed.",
                    response.ConversationId,
                    response.BasketId,
                    response.Price,
                    response.PaidPrice,
                    response.Currency)
                {
                    VerificationStatus = PaymentVerificationStatus.Unknown,
                    FraudStatus = response.FraudStatus
                };
            }
            var verificationStatus = response.FraudStatus switch
            {
                1 when isPaymentSuccessful =>
                    PaymentVerificationStatus.Succeeded,

                -1 => PaymentVerificationStatus.Failed,

                0 => PaymentVerificationStatus.Pending,

                _ => PaymentVerificationStatus.Unknown
            };

            return new PaymentCheckoutResult(
                verificationStatus == PaymentVerificationStatus.Succeeded,
                response.PaymentId,
                response.ErrorMessage,
                response.ConversationId,
                response.BasketId,
                response.Price,
                response.PaidPrice,
                response.Currency)
            {
                VerificationStatus = verificationStatus,
                FraudStatus = response.FraudStatus
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve iyzico payment. PaymentId: {PaymentId}, ConversationId: {ConversationId}",
                providerPaymentId,
                paymentConversationId);

            return new PaymentCheckoutResult(
                false,
                null,
                "Unable to retrieve the iyzico payment.",
                null, null, null, null, null)
            {
                VerificationStatus = PaymentVerificationStatus.Unknown
            };
        }
    }

}
