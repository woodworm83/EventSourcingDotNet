using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet.UnitTests;

[SuppressMessage("ReSharper", "WithExpressionModifiesAllMembers")]
internal sealed record TestEvent(int NewValue = default) : IDomainEvent;