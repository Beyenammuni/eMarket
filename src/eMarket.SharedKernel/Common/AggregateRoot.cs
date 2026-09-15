using System.Diagnostics;
using System.Collections.Generic;
using eMarket.SharedKernel.DomainEvent;

namespace  eMarket.SharedKernel.Common;

/// <summary>
/// Represents an Aggregate Root in the domain.
/// </summary>
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
    where TKey : IEquatable<TKey>;
