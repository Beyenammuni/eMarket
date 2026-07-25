using eMarket.Domain.Catalog.ValueObjects;
using eMarket.SharedKernel.Rules;

namespace eMarket.Domain.Catalog.Rules;

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
