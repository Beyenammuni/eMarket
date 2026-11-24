
using eMarket.Domain.Businesses.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Businesses;

public sealed class BusinessMemberPermissionConfiguration
    : IEntityTypeConfiguration<BusinessMemberPermission>
{
    public void Configure(
        EntityTypeBuilder<BusinessMemberPermission> builder)
    {
        builder.ToTable("BusinessMemberPermissions");

        builder.HasKey(x => new
        {
            x.BusinessMemberId,
            x.Permission
        });

        builder.Property(x => x.BusinessMemberId)
            .HasConversion(
                id => id.Value,
                value => BusinessMemberId.Create(value))
            .IsRequired();

        builder.Property(x => x.Permission)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.IsGranted)
            .IsRequired();

        builder.HasOne<BusinessMember>()
    .WithMany()
    .HasForeignKey(x => x.BusinessMemberId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.BusinessMemberId);
    }
}
