using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

[CollectionDefinition(nameof(EventStoreCollection))]
public sealed class EventStoreCollection : ICollectionFixture<EventStoreFixture>
{
}