using System.Linq.Expressions;

namespace eMarket.Application.Common.Specifications;

public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>>? Criteria { get; }

    List<Expression<Func<TEntity, object>>> Includes { get; }

    Expression<Func<TEntity, object>>? OrderBy { get; }

    Expression<Func<TEntity, object>>? OrderByDescending { get; }

    int Skip { get; }

    int Take { get; }

    bool IsPagingEnabled { get; }
}
