using eMarket.SharedKernel.DomainEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Domain.Catalog.Events
{
    public sealed record CategoryCreatedDomainEvent(CategoryId CategoryId)
  : DomainEvent;
}
