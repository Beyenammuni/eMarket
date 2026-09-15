using System.Globalization;
using eMarket.Application.Common.Interfaces.Payments;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;

namespace eMarket.Infrastructure.Payments.Iyzico;

internal sealed class IyzicoPaymentGateway : IPaymentGateway
{
    private readonly IyzicoOptions _options;

    public IyzicoPaymentGateway(
        IOptions<IyzicoOptions> options)
    {
        _options = options.Value;
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
            GsmNumber = request.Buyer.Phone,
            Email = request.Buyer.Email,
            Country = "Turkey"
        };

        var shippingAddress = CreateAddress(
            request.DeliveryAddress);

        var billingAddress = CreateAddress(
            request.DeliveryAddress);

        var basketItems = CreateBasketItems(request);

        var checkoutRequest =
            new CreateCheckoutFormInitializeRequest
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
                BasketId = request.OrderId.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = request.ReturnUrl,

                Buyer = buyer,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                BasketItems = basketItems
            };

        try
        {
            var response =
                await CheckoutFormInitialize.Create(
                    checkoutRequest,
                    options);

            if (!string.Equals(
                    response.Status,
                    Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return new PaymentGatewayResult(
                    false,
                    null,
                    response.Token,
                    response.ErrorMessage ??
                    "iyzico marketplace checkout initialization failed.");
            }

            return new PaymentGatewayResult(
                true,
                response.PaymentPageUrl,
                response.Token,
                null);
        }
        catch (Exception ex)
        {
            return new PaymentGatewayResult(
                false,
                null,
                null,
                ex.Message);
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
}
