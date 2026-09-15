using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Identity;
using eMarket.Domain.Subscriptions;
using eMarket.Domain.Subscriptions.ValueObjects;
using FluentAssertions;

namespace eMarket.UnitTests.Subscriptions;

public sealed class SubscriptionTests
{
    [Fact]
    public void Create_ShouldCreateActiveSubscription()
    {
        var result = Subscription.Create(
            UserId.New(),
            BusinessId.New(),
            DayOfWeek.Saturday,
            DateTime.UtcNow.Date,
            CreateDeliveryAddress());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void Create_ShouldStoreDeliveryAddress()
    {
        var address = CreateDeliveryAddress();

        var result = Subscription.Create(
            UserId.New(),
            BusinessId.New(),
            DayOfWeek.Saturday,
            DateTime.UtcNow.Date,
            address);

        result.IsSuccess.Should().BeTrue();

        var subscription = result.Value!;

        subscription.DeliveryAddress.Should().NotBeNull();
        subscription.DeliveryAddress.FullName.Should().Be("John Doe");
        subscription.DeliveryAddress.PhoneNumber.Should().Be("123-456-7890");
        subscription.DeliveryAddress.AddressLine.Should().Be("123 Main St");
        subscription.DeliveryAddress.City.Should().Be("New York");
        subscription.DeliveryAddress.District.Should().Be("Manhattan");
        subscription.DeliveryAddress.PostalCode.Should().Be("10001");
        subscription.DeliveryAddress.Latitude.Should().Be(40.7128m);
        subscription.DeliveryAddress.Longitude.Should().Be(-74.0060m);
    }

    [Fact]
    public void Skip_ShouldMoveNextDeliveryBySevenDays()
    {
        var date = DateTime.UtcNow.Date;

        var subscription = Subscription.Create(
            UserId.New(),
            BusinessId.New(),
            DayOfWeek.Saturday,
            date,
            CreateDeliveryAddress()).Value!;

        subscription.AddOrUpdateItem(
            ProductId.New(),
            2);

        subscription.SkipNextDelivery();

        subscription.NextDeliveryDate.Should().Be(date.AddDays(7));
    }

    [Fact]
    public void Cancel_ShouldPreventFurtherDelivery()
    {
        var subscription = Subscription.Create(
            UserId.New(),
            BusinessId.New(),
            DayOfWeek.Saturday,
            DateTime.UtcNow.Date,
            CreateDeliveryAddress()).Value!;

        subscription.AddOrUpdateItem(
            ProductId.New(),
            1);

        subscription.Cancel();

        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.SkipNextDelivery().IsFailure.Should().BeTrue();
    }

    private static SubscriptionDeliveryAddress CreateDeliveryAddress()
    {
        var result = SubscriptionDeliveryAddress.Create(
            "John Doe",
            "123-456-7890",
            "123 Main St",
            "New York",
            "Manhattan",
            "10001",
            "Downtown",
            "Main St",
            "B1",
            "Apt 2",
            40.7128m,
            -74.0060m);

        result.IsSuccess.Should().BeTrue();

        return result.Value!;
    }
}
