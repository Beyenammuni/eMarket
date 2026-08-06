using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.Infrastructure.Persistence.Configurations.Base;
using eMarket.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Businesses;

public sealed class BusinessConfiguration
    : EntityConfiguration<Business, BusinessId>
{
    protected override BusinessId CreateId(Guid value)
        => BusinessId.Create(value);

    protected override void ConfigureEntity(
        EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses");

        builder.OwnsOne(x => x.Name, name =>
        {
            name.Property(x => x.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Navigation(x => x.Members)
       .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Business.Members))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.Members, member =>
        {
            member.ToTable("BusinessMembers");

            member.WithOwner()
                .HasForeignKey("BusinessId");

            member.HasKey(x => x.Id);

            member.Property(x => x.Id)
                .HasConversion(
                    new StronglyTypedIdConverter<BusinessMemberId>(BusinessMemberId.Create))
                .Metadata.SetValueComparer(
                    new StronglyTypedIdComparer<BusinessMemberId>());

            member.Property(x => x.Role)
      .HasConversion(new EnumerationConverter<BusinessRole>());

            builder.Property(x => x.Status)
    .HasConversion<string>()
    .IsRequired();


            builder.Property(x => x.Type)
       .HasConversion(new EnumerationConverter<BusinessType>());
            member.Property(x => x.Id)
                .ValueGeneratedNever();

            member.Property(x => x.UserId)
          .HasConversion(
           new StronglyTypedIdConverter<UserId>(UserId.Create))
           .Metadata.SetValueComparer(
           new StronglyTypedIdComparer<UserId>());

            member.Property(x => x.JoinedAt)
           .IsRequired();

            member.Property(x => x.IsActive)
           .IsRequired();
            member.HasIndex(x => new { x.UserId, x.IsActive });
        });
    }
}
