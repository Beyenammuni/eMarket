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
    }
}
