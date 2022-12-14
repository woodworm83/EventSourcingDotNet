using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class EventStoreFixture : IAsyncLifetime
{
    public EventStoreTestContainer Container { get; } = EventStoreTestContainer.BuildContainer();
    
    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(EventStoreCollection))]
public class EventStoreCollection : ICollectionFixture<EventStoreFixture>
{
}