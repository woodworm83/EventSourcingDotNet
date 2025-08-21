using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

[CollectionDefinition(nameof(EventStoreCollection))]
public class EventStoreCollection : ICollectionFixture<EventStoreFixture>
{
}