using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.Domain.Catalog.Categories.Events;
using eMarket.Domain.Catalog.Categories.Rules;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;


namespace eMarket.Domain.Catalog.Categories;

public sealed class Category : AggregateRoot<CategoryId>
{
    private Category()
    {
    }

    private Category( 
        CategoryId id,
        CategoryName name)
    {
        Id = id;
        Name = name;
        Status = CategoryStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public CategoryName Name { get; private set; } = default!;

    public CategoryStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Result<Category> Create(CategoryName name)
    {
        var category = new Category(
            CategoryId.New(),
            name);

        category.AddDomainEvent(
            new CategoryCreatedDomainEvent(
                category.Id));

        return Result<Category>.Success(category);
    }

    public Result Rename(CategoryName newName)
    {
        if (new CategoryCannotHaveSameNameRule(Name, newName).IsBroken())
        {
            return Result.Failure(CategoryErrors.SameName);
        }


        Name = newName;
        AddDomainEvent(
            new CategoryRenamedDomainEvent(
                Id,
                newName));

        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == CategoryStatus.Active)
        {
            return Result.Failure(CategoryErrors.AlreadyActive);
        }

        Status = CategoryStatus.Active;

        AddDomainEvent(
            new CategoryActivatedDomainEvent(Id));

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == CategoryStatus.Inactive)
        {
            return Result.Failure(CategoryErrors.AlreadyInactive);
        }
        Status = CategoryStatus.Inactive;

        AddDomainEvent(
            new CategoryDeactivatedDomainEvent(Id));

        return Result.Success();
    }
}
