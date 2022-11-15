using Xunit;

namespace Streamy.EventStore.UnitTests;

public class EventStoreFixture : IAsyncLifetime
{
    public EventStoreTestContainer Container { get; } = EventStoreTestContainer.BuildContainer();
    
    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
    }
}

[CollectionDefinition(nameof(EventStoreCollection))]
public class EventStoreCollection : ICollectionFixture<EventStoreFixture>
{
}