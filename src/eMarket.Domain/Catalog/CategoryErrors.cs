using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Catalog;

public static class CategoryErrors
{
    public static readonly Error NameIsRequired =
        new(
            "Category.Name.Required",
            "Category name is required.");

    public static readonly Error NameTooLong =
        new(
            "Category.Name.TooLong",
            "Category name cannot exceed 100 characters.");

    public static readonly Error SameName =
        new(
            "Category.Name.Same",
            "Category already has this name.");

    public static readonly Error AlreadyActive =
        new(
            "Category.Status.Active",
            "Category is already active.");

    public static readonly Error AlreadyInactive =
        new(
            "Category.Status.Inactive",
            "Category is already inactive.");
}
