using eMarket.Infrastructure.Persistence.Converters;
using eMarket.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Base;

public abstract class EntityConfiguration<TEntity, TId>
    : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity<TId>
    where TId : StronglyTypedId
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                new StronglyTypedIdConverter<TId>(CreateId))
            .Metadata.SetValueComparer(
                new StronglyTypedIdComparer<TId>());

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Ignore(x => x.DomainEvents);

        ConfigureEntity(builder);
    }

    protected abstract TId CreateId(Guid value);

    protected abstract void ConfigureEntity(
        EntityTypeBuilder<TEntity> builder);
}
