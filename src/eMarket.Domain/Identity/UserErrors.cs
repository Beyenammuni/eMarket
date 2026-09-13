using eMarket.SharedKernel.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Domain.Identity
{

    public static class UserErrors
    {
        public static readonly Error DuplicateRole = new(
            "User.DuplicateRole",
            "The user already has this role.");

        public static readonly Error LastRoleCannotBeRemoved = new(
            "User.LastRoleCannotBeRemoved",
            "A user must have at least one role.");

        public static readonly Error EmailAlreadyVerified = new(
            "User.EmailAlreadyVerified",
            "Email has already been verified.");

        public static readonly Error SameEmail = new(
            "User.SameEmail",
            "The new email must be different from the current email.");

        public static readonly Error UsernameRequired = new(
            "User.Username.Required",
            "Username is required.");

        public static readonly Error UsernameInvalid = new(
            "User.Username.Invalid",
            "Username must be between 3 and 50 characters.");

        public static readonly Error UsernameExists = new(
            "User.Username.Exists",
            "Username is already registered.");

        public static readonly Error InvalidResetToken = new(
            "Auth.InvalidResetToken",
            "The password reset token is invalid or expired.");

        public static readonly Error SamePassword = new(
            "Auth.SamePassword",
            "The new password must be different from the current password.");

        public static readonly Error InvalidCredentials = new(
    "User.InvalidCredentials",
    "Invalid email or password.");

        public static readonly Error AccountNotActive = new(
            "User.AccountNotActive",
            "User account is not active.");
    }
}
