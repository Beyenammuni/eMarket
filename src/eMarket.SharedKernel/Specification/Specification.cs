using System.Linq.Expressions;

namespace EMarket.SharedKernel.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }

    public List<Expression<Func<T, object>>> Includes { get; } = new();

    public Expression<Func<T, object>>? OrderBy { get; protected set; }

    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }

    public int Skip { get; protected set; }

    public int Take { get; protected set; }

    public bool IsPagingEnabled { get; protected set; }

    protected void AddInclude(Expression<Func<T, object>> include)
    {
        Includes.Add(include);
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> expression)
    {
        OrderBy = expression;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> expression)
    {
        OrderByDescending = expression;
    }
}