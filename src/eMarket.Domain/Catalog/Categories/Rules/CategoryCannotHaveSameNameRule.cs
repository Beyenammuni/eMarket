using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Catalog.Categories.Rules;

public sealed class CategoryCannotHaveSameNameRule
    : IBusinessRule
{
    private readonly CategoryName _current;
    private readonly CategoryName _new;

    public CategoryCannotHaveSameNameRule(
        CategoryName current,
        CategoryName @new)
    {
        _current = current;
        _new = @new;
    }

    public string Message =>
        "Category already has this name.";

    public bool IsBroken()
        => _current == _new;
}
