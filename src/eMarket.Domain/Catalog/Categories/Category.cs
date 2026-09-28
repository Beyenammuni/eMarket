using eMarket.Domain.Catalog.Categories.Events;
using eMarket.Domain.Catalog.Categories.Rules;
using eMarket.Domain.Catalog.Categories.ValueObjects;
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
        Guid businessId,
        CategoryName name,
        CategoryId? parentCategoryId)
    {
        Id = id;
        BusinessId = businessId;
        Name = name;
        ParentCategoryId = parentCategoryId;
        Status = CategoryStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid BusinessId { get; private set; }

    public CategoryName Name { get; private set; } = default!;

    public CategoryId? ParentCategoryId { get; private set; }

    public CategoryStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Result<Category> Create(
        Guid businessId,
        CategoryName name,
        CategoryId? parentCategoryId = null)
    {

        var category = new Category(
            CategoryId.New(),
            businessId,
            name,
            parentCategoryId);

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
