using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Subscriptions;
using eMarket.Domain.Subscriptions.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Subscriptions.Commands.CreateSubscription;

internal sealed class CreateSubscriptionCommandHandler
    : IRequestHandler<CreateSubscriptionCommand, Result<CreateSubscriptionResponse>>
{
    private readonly ISubscriptionDbContext _context;
    private readonly ICatalogDbContext _catalog;
    private readonly ICurrentUser _currentUser;

    public CreateSubscriptionCommandHandler(
        ISubscriptionDbContext context,
        ICatalogDbContext catalog,
        ICurrentUser currentUser)
    {
        _context = context;
        _catalog = catalog;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateSubscriptionResponse>> Handle(
        CreateSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
        {
            return Result<CreateSubscriptionResponse>.Failure(
                new Error(
                    "Auth.Unauthorized",
                    "User is not authenticated."));
        }

        var businessId = BusinessId.Create(request.BusinessId);

        var productIds = request.Items
            .Select(x => ProductId.Create(x.ProductId))
            .ToList();

        var products = await _catalog.Products
            .Where(x =>
                productIds.Contains(x.Id) &&
                x.BusinessId == businessId &&
                x.Status == ProductStatus.Active)
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return Result<CreateSubscriptionResponse>.Failure(
                SubscriptionErrors.DifferentBusinesses);
        }

        var addressResult = SubscriptionDeliveryAddress.Create(
            request.FullName,
            request.PhoneNumber,
            request.AddressLine,
            request.City,
            request.District,
            request.PostalCode,
            request.Neighborhood,
            request.Street,
            request.BuildingNumber,
            request.ApartmentNumber,
            request.Latitude,
            request.Longitude);

        if (addressResult.IsFailure)
        {
            return Result<CreateSubscriptionResponse>.Failure(
                addressResult.Error);
        }

        var deliveryAddress = addressResult.Value!;

        var result = Subscription.Create(
            _currentUser.UserId,
            businessId,
            request.DeliveryDay,
            request.FirstDeliveryDate,
            deliveryAddress);

        if (result.IsFailure)
        {
            return Result<CreateSubscriptionResponse>.Failure(
                result.Error);
        }

        var subscription = result.Value!;

        foreach (var item in request.Items)
        {
            var addResult = subscription.AddOrUpdateItem(
                ProductId.Create(item.ProductId),
                item.Quantity);

            if (addResult.IsFailure)
            {
                return Result<CreateSubscriptionResponse>.Failure(
                    addResult.Error);
            }
        }

        await _context.Subscriptions.AddAsync(
            subscription,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result<CreateSubscriptionResponse>.Success(
            new(
                subscription.Id.Value,
                businessId.Value,
                subscription.DeliveryDay,
                subscription.NextDeliveryDate,
                subscription.Status.ToString()));
    }
}
