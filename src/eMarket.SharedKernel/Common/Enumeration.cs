using System.Reflection;

namespace EMarket.SharedKernel.Common;

public abstract class Enumeration : IComparable
{
    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }

    public string Name { get; }

    public override string ToString()
        => Name;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration other)
            return false;

        return GetType() == other.GetType()
            && Id == other.Id;
    }

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);

    public int CompareTo(object? other)
        => Id.CompareTo(((Enumeration)other!).Id);

    public static IEnumerable<T> GetAll<T>()
        where T : Enumeration
    {
        return typeof(T)
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }

    public static T FromId<T>(int id)
        where T : Enumeration
    {
        var item = GetAll<T>()
            .FirstOrDefault(x => x.Id == id);

        return item
            ?? throw new InvalidOperationException(
                $"'{id}' is not valid for {typeof(T).Name}");
    }

    public static T FromName<T>(string name)
        where T : Enumeration
    {
        var item = GetAll<T>()
            .FirstOrDefault(x =>
                string.Equals(
                    x.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase));

        return item
            ?? throw new InvalidOperationException(
                $"'{name}' is not valid for {typeof(T).Name}");
    }
    public static TEnumeration FromValue<TEnumeration>(int value)
    where TEnumeration : Enumeration
    {
        var matchingItem = GetAll<TEnumeration>()
            .FirstOrDefault(x => x.Id == value);

        if (matchingItem is null)
            throw new InvalidOperationException(
                $"'{value}' is not a valid value for {typeof(TEnumeration).Name}");

        return matchingItem;
    }
}
