using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Identity;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Subscriptions.Queries.GetBusinessSubscriptions;

internal sealed class GetBusinessSubscriptionsQueryHandler
    : IRequestHandler<
        GetBusinessSubscriptionsQuery,
        Result<IReadOnlyCollection<GetBusinessSubscriptionResponse>>>
{
    private readonly ISubscriptionDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    public GetBusinessSubscriptionsQueryHandler(
        ISubscriptionDbContext context,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _context = context;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    public async Task<
        Result<IReadOnlyCollection<GetBusinessSubscriptionResponse>>>
        Handle(
            GetBusinessSubscriptionsQuery request,
            CancellationToken ct)
    {
        var businessId =
            BusinessId.Create(request.BusinessId);

        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x =>
                x.BusinessId == businessId)
            .ToListAsync(ct);

        var userIds = subscriptions
            .Select(x => x.UserId.Value)
            .Distinct()
            .ToList();

        var users = new Dictionary<Guid, User>();

        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(
                UserId.Create(userId),
                ct);

            if (user is not null)
            {
                users[userId] = user;
            }
        }
        var productIds = subscriptions
    .SelectMany(x => x.Items)
    .Select(x => x.ProductId.Value)
    .Distinct()
    .ToList();

        var products = new Dictionary<Guid, string>();

        foreach (var productId in productIds)
        {
            var product = await _productRepository.GetByIdAsync(
                new Domain.Catalog.Products.ProductId(productId),
                businessId,
                ct);

            if (product is not null)
            {
                products[productId] = product.Name.ToString();
            }
        }

        var result = subscriptions
            .Select(x =>
            {
                users.TryGetValue(
                    x.UserId.Value,
                    out var user);

                return new GetBusinessSubscriptionResponse(
                    x.Id.Value,
                    x.BusinessId.Value,
                    x.UserId.Value,
                    user?.FullName.ToString()
                        ?? "Bilinmeyen Kullanıcı",
                    user?.Username
                        ?? "Bilinmiyor",
                    x.DeliveryDay,
                    x.NextDeliveryDate,
                    x.Status.ToString(),
                    x.LastGeneratedOrderId,
                    new SubscriptionDeliveryAddressResponse(
                        x.DeliveryAddress.FullName,
                        x.DeliveryAddress.PhoneNumber,
                        x.DeliveryAddress.AddressLine,
                        x.DeliveryAddress.City,
                        x.DeliveryAddress.District,
                        x.DeliveryAddress.PostalCode,
                        x.DeliveryAddress.Neighborhood,
                        x.DeliveryAddress.Street,
                        x.DeliveryAddress.BuildingNumber,
                        x.DeliveryAddress.ApartmentNumber,
                        x.DeliveryAddress.Latitude,
                        x.DeliveryAddress.Longitude),
                   x.Items
    .Select(i =>
        new SubscriptionItemResponse(
            i.ProductId.Value,
            products.TryGetValue(
                i.ProductId.Value,
                out var productName)
                ? productName
                : "Bilinmeyen Ürün",
            i.Quantity))
    .ToList());
            })
            .ToList();

        return Result<
            IReadOnlyCollection<GetBusinessSubscriptionResponse>>
            .Success(result);
    }
}
