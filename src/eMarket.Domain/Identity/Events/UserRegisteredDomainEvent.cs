using eMarket.SharedKernel.DomainEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Domain.Identity.Events
{
    public sealed record UserRegisteredDomainEvent(UserId userId) : DomainEvent;
}
