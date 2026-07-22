using EMarket.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Domain.Identity
{
    public sealed class UserStatus : Enumeration
    {
        public static readonly UserStatus Pending = new(1, nameof(Pending));
        public static readonly UserStatus Active = new(2, nameof(Active));
        public static readonly UserStatus Suspended = new(3, nameof(Suspended));
        public static readonly UserStatus Deleted = new(4, nameof(Deleted));

        private UserStatus(int id, string name)
            : base(id, name)
        {
        }
    }
}
