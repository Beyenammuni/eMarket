using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using eMarket.Domain.Identity.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace eMarket.Infrastructure.Identity;

public sealed class AdminSeeder(
    IIdentityDbContext context,
    IPasswordHasher passwordHasher,
    IConfiguration configuration)
{
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        var email = configuration["AdminSeed:Email"];
        var username = configuration["AdminSeed:Username"];
        var password = configuration["AdminSeed:Password"];
        var fullName = configuration["AdminSeed:FullName"];
        var phoneNumber = configuration["AdminSeed:PhoneNumber"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new InvalidOperationException(
                "AdminSeed configuration is incomplete.");
        }

        var nameParts = fullName
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (nameParts.Length < 2)
        {
            throw new InvalidOperationException(
                "AdminSeed:FullName must contain first name and last name.");
        }

        var firstName = nameParts[0];
        var lastName = string.Join(' ', nameParts.Skip(1));

        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        var normalizedUsername =
            username.Trim().ToLowerInvariant();

        var existingUser = await context.Users
            .FirstOrDefaultAsync(
                x => x.Email.Value == normalizedEmail ||
                     x.Username == normalizedUsername,
                cancellationToken);

        if (existingUser is not null)
        {
            var hasSuperAdminRole =
                await context.UserRoleAssignments
                    .AnyAsync(
                        x => x.UserId == existingUser.Id &&
                             x.RoleId == UserRole.SuperAdmin.Id,
                        cancellationToken);

            if (!hasSuperAdminRole)
            {
                await context.UserRoleAssignments.AddAsync(
                    new UserRoleAssignment(
                        existingUser.Id,
                        UserRole.SuperAdmin),
                    cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var fullNameValue =
            FullName.Create(firstName, lastName);

        var emailValue =
            Email.Create(normalizedEmail);

        var phoneValue =
            PhoneNumber.Create(phoneNumber);

        var userResult = User.Register(
            fullNameValue,
            emailValue,
            phoneValue,
            normalizedUsername);

        if (userResult.IsFailure)
        {
            throw new InvalidOperationException(
                userResult.Error.Description);
        }

        var user = userResult.Value;

        // Remove the default Customer role.
        var customerRole = user.Roles
            .FirstOrDefault(x => x.Id == UserRole.Customer.Id);

        if (customerRole is not null)
        {
            // Allow removing the last role here because the seeder will
            // assign the SuperAdmin role immediately afterwards.
            var removeResult =
                user.RemoveRole(customerRole, allowRemovingLast: true);

            if (removeResult.IsFailure)
            {
                throw new InvalidOperationException(
                    removeResult.Error.Description);
            }
        }

        // Add SuperAdmin role to the domain aggregate.
        var superAdminResult =
            user.AssignRole(UserRole.SuperAdmin);

        if (superAdminResult.IsFailure)
        {
            throw new InvalidOperationException(
                superAdminResult.Error.Description);
        }

        await context.Users.AddAsync(
            user,
            cancellationToken);

        var passwordHash =
            passwordHasher.Hash(password);

        await context.UserCredentials.AddAsync(
            new UserCredential(
                user.Id,
                passwordHash),
            cancellationToken);

        await context.UserRoleAssignments.AddAsync(
            new UserRoleAssignment(
                user.Id,
                UserRole.SuperAdmin),
            cancellationToken);

        await context.SaveChangesAsync(
            cancellationToken);
    }
}
