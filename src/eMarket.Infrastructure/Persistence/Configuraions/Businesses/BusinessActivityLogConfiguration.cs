
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Businesses;

public sealed class BusinessActivityLogConfiguration
    : IEntityTypeConfiguration<BusinessActivityLog>
{
    public void Configure(EntityTypeBuilder<BusinessActivityLog> builder)
    {
        builder.ToTable("BusinessActivityLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessId)
            .HasConversion(
                id => id.Value,
                value => BusinessId.Create(value))
            .IsRequired();

        builder.Property(x => x.ActorUserId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .IsRequired();

        builder.Property(x => x.TargetUserId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : UserId.Create(value.Value));

        builder.Property(x => x.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.BusinessId, x.CreatedAt });

        builder.HasIndex(x => x.TargetUserId);
    }
}
