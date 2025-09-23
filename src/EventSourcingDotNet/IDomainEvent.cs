using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet;

public interface IDomainEvent;

[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed")]
public interface IDomainEvent<[UsedImplicitly] TAggregateId> : IDomainEvent
    where TAggregateId : IAggregateId
{
}