using eMarket.Application.Common.Interfaces.Payments;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Model.V2.Subscription;
using Iyzipay.Request.V2.Subscription;
using Microsoft.Extensions.Options;

namespace eMarket.Infrastructure.Payments.Iyzico;

internal sealed class IyzicoSubscriptionGateway
    : ISubscriptionPaymentGateway
{
    private readonly IyzicoOptions _options;

    public IyzicoSubscriptionGateway(
        IOptions<IyzicoOptions> options)
    {
        _options = options.Value;
    }

    public async Task<SubscriptionPaymentGatewayResult>
        CreateCheckoutFormAsync(
            SubscriptionPaymentGatewayRequest request,
            CancellationToken cancellationToken = default)
    {
        var options = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl
        };

        try
        {
            var pricingPlanReferenceCode =
                await GetOrCreatePricingPlanAsync(
                    request.MonthlyAmount,
                    request.Currency,
                    options);

            if (string.IsNullOrWhiteSpace(pricingPlanReferenceCode))
            {
                return new SubscriptionPaymentGatewayResult(
                    false,
                    null,
                    null,
                    "Could not find or create an iyzico subscription pricing plan.");
            }

            var customer = new CheckoutFormCustomer
            {
                Name = request.Buyer.Name,
                Surname = request.Buyer.Surname,
                Email = request.Buyer.Email,
                GsmNumber = request.Buyer.Phone,
                IdentityNumber = request.Buyer.IdentityNumber,
                BillingAddress = CreateAddress(request.Address),
                ShippingAddress = CreateAddress(request.Address)
            };

            var checkoutRequest =
                new InitializeCheckoutFormRequest
                {
                    ConversationId = request.PaymentId.ToString(),
                    CallbackUrl = request.ReturnUrl,
                    PricingPlanReferenceCode =
                        pricingPlanReferenceCode,
                    SubscriptionInitialStatus = "ACTIVE",
                    Customer = customer
                };

            var response =
                Subscription.InitializeCheckoutForm(
                    checkoutRequest,
                    options);

            if (!string.Equals(
                    response.Status,
                    Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return new SubscriptionPaymentGatewayResult(
                    false,
                    null,
                    null,
                    response.ErrorMessage ??
                    "iyzico subscription checkout initialization failed.");
            }

            return new SubscriptionPaymentGatewayResult(
                true,
                response.CheckoutFormContent,
                response.Token,
                null);
        }
        catch (Exception ex)
        {
            return new SubscriptionPaymentGatewayResult(
                false,
                null,
                null,
                ex.Message);
        }
    }

    private async Task<string?> GetOrCreatePricingPlanAsync(
    decimal monthlyAmount,
    string currency,
    Iyzipay.Options options)
    {
        var productReferenceCode =
            _options.SubscriptionProductReferenceCode;

        if (string.IsNullOrWhiteSpace(productReferenceCode))
        {
            var createProductRequest =
                new CreateProductRequest
                {
                    ConversationId = Guid.NewGuid().ToString(),
                    Name = "eMarket",
                    Description =
                        "eMarket monthly recurring subscription."
                };

            var productResponse =
                Product.Create(
                    createProductRequest,
                    options);
            if (!string.Equals(
        productResponse.Status,
        Status.SUCCESS.ToString(),
        StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"iyzico Product.Create failed. " +
                    $"Status: {productResponse.Status}, " +
                    $"ErrorCode: {productResponse.ErrorCode}, " +
                    $"ErrorMessage: {productResponse.ErrorMessage}");
            }

            productReferenceCode =
                productResponse.Data?.ReferenceCode;

            if (string.IsNullOrWhiteSpace(productReferenceCode))
            {
                throw new InvalidOperationException(
                    "Pricing plan failed: ProductReferenceCode is empty.");
            }
        }

        var plansRequest =
            new RetrieveAllPlanRequest
            {
                ConversationId = Guid.NewGuid().ToString(),
                ProductReferenceCode = productReferenceCode,
                Page = 1,
                Count = 100
            };

        var plansResponse =
            Plan.RetrieveAll(
                plansRequest,
                options);

        if (!string.Equals(
        plansResponse.Status,
        Status.SUCCESS.ToString(),
        StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"iyzico Plan.RetrieveAll failed. " +
                $"Status: {plansResponse.Status}, " +
                $"ErrorCode: {plansResponse.ErrorCode}, " +
                $"ErrorMessage: {plansResponse.ErrorMessage}");
        }

        var existingPlan =
            plansResponse.Data?.Items?
                .FirstOrDefault(x =>
                    decimal.TryParse(
                        x.Price,
                        out var price) &&
                    price == monthlyAmount &&
                    string.Equals(
                        x.CurrencyCode,
                        currency,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        x.PaymentInterval,
                        "MONTHLY",
                        StringComparison.OrdinalIgnoreCase) &&
                    x.PaymentIntervalCount == 1 &&
                    string.Equals(
                        x.PlanPaymentType,
                        "RECURRING",
                        StringComparison.OrdinalIgnoreCase));

        if (existingPlan is not null)
            return existingPlan.ReferenceCode;

        var createPlanRequest =
            new CreatePlanRequest
            {
                ConversationId = Guid.NewGuid().ToString(),
                ProductReferenceCode = productReferenceCode,
                Name = $"eMarket Monthly {monthlyAmount:0.00} {currency}",
                Price = monthlyAmount.ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture),
                CurrencyCode = currency,
                PaymentInterval = "MONTHLY",
                PaymentIntervalCount = 1,
                TrialPeriodDays = 0,
                PlanPaymentType = "RECURRING",
                RecurrenceCount = null
            };

        var createPlanResponse =
            Plan.Create(
                createPlanRequest,
                options);

        if (!string.Equals(
        createPlanResponse.Status,
        Status.SUCCESS.ToString(),
        StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"iyzico Plan.Create failed. " +
                $"Status: {createPlanResponse.Status}, " +
                $"ErrorCode: {createPlanResponse.ErrorCode}, " +
                $"ErrorMessage: {createPlanResponse.ErrorMessage}");
        }

        return createPlanResponse.Data?.ReferenceCode;
    }
    
    private static Address CreateAddress(
        SubscriptionPaymentAddress address)
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
        SubscriptionPaymentAddress address)
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
    public async Task<string> TestListProductsAsync(
    CancellationToken cancellationToken = default)
    {
        var options = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl
        };

        var request = new PagingRequest
        {
            Page = 1,
            Count = 10
        };

        var response = Product.RetrieveAll(request, options);

        return System.Text.Json.JsonSerializer.Serialize(response);
    }
}
