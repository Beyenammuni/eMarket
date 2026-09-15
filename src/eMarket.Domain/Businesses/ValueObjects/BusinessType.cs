using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Businesses.ValueObjects;

public sealed class BusinessType : Enumeration
{
    public static readonly BusinessType Grocery =
        new(1, nameof(Grocery));

    public static readonly BusinessType Supermarket =
        new(2, nameof(Supermarket));

    public static readonly BusinessType Bakery =
        new(3, nameof(Bakery));

    public static readonly BusinessType Restaurant =
        new(4, nameof(Restaurant));

    public static readonly BusinessType Pharmacy =
        new(5, nameof(Pharmacy));

    public static readonly BusinessType Cosmetics =
        new(6, nameof(Cosmetics));

    public static readonly BusinessType Electronics =
        new(7, nameof(Electronics));

    public static readonly BusinessType Clothing =
        new(8, nameof(Clothing));

    public static readonly BusinessType HomeAndFurniture =
        new(9, nameof(HomeAndFurniture));

    public static readonly BusinessType Butcher =
        new(10, nameof(Butcher));

    public static readonly BusinessType FishMarket =
        new(11, nameof(FishMarket));

    public static readonly BusinessType FruitAndVegetable =
        new(12, nameof(FruitAndVegetable));

    public static readonly BusinessType Cafe =
        new(13, nameof(Cafe));

    public static readonly BusinessType SweetsAndDesserts =
        new(14, nameof(SweetsAndDesserts));

    public static readonly BusinessType PetShop =
        new(15, nameof(PetShop));

    public static readonly BusinessType Bookstore =
        new(16, nameof(Bookstore));

    public static readonly BusinessType Jewelry =
        new(17, nameof(Jewelry));

    public static readonly BusinessType SportsAndFitness =
        new(18, nameof(SportsAndFitness));

    public static readonly BusinessType FlowersAndGifts =
        new(19, nameof(FlowersAndGifts));

    public static readonly BusinessType Other =
        new(20, nameof(Other));

    private BusinessType(
        int id,
        string name)
        : base(id, name)
    {
    }
}
