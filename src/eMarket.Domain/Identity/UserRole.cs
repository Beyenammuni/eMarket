using EMarket.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Domain.Identity
{
    public sealed class UserRole : Enumeration
    {
        public static readonly UserRole Customer = new(1, nameof(Customer));
        public static readonly UserRole Merchant = new(2, nameof(Merchant));
        public static readonly UserRole Driver = new(3, nameof(Driver));
        public static readonly UserRole Admin = new(4, nameof(Admin));

        private UserRole(int id, string name)
            : base(id, name)
        {
        }
    }
}
