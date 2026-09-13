using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Common.Interfaces;

public interface IIdentityDbContext
{
    DbSet<User> Users { get; }

    DbSet<UserCredential> UserCredentials { get; }

    DbSet<PasswordResetToken> PasswordResetTokens { get; }

    DbSet<UserRoleAssignment> UserRoleAssignments { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
