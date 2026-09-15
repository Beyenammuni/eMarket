using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.DomainEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Domain.Identity.Events
{
    public sealed record UserEmailChangedDomainEvent(UserId userId, Email newEmail) : DomainEvent;
}
