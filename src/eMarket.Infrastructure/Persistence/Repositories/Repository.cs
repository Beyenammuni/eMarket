using eMarket.Application.Common.Specifications;
using eMarket.SharedKernel.Common;
using eMarket.Infrastructure.Persistence.Specifications;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public class Repository<TEntity, TId>
    where TEntity : Entity<TId>
{
    protected readonly AppDbContext Context;

    public Repository(AppDbContext context)
    {
        Context = context;
    }

    public async Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>()
            .FirstOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken);
    }

    public async Task<List<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        var query = SpecificationEvaluator.GetQuery(
            Context.Set<TEntity>().AsQueryable(),
            specification);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>()
            .AddAsync(entity, cancellationToken);
    }
}
