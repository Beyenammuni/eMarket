using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using eMarket.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.ToTable("UserRoleAssignments");
        builder.HasKey(x => new { x.UserId, x.RoleId });
        builder.Property(x => x.UserId)
            .HasConversion(new StronglyTypedIdConverter<UserId>(UserId.Create))
            .Metadata.SetValueComparer(new StronglyTypedIdComparer<UserId>());
        builder.Property(x => x.RoleId).IsRequired();
        builder.HasIndex(x => x.RoleId);
    }
}
