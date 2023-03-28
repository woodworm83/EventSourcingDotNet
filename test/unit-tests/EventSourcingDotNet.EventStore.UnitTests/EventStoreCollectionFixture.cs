using DotNet.Testcontainers.Containers;
using EventStore.Client;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class EventStoreFixture : IAsyncLifetime
{
    public IContainer Container { get; } = EventStoreTestContainer.BuildContainer();
    
    public EventStoreClientSettings ClientSettings
        => EventStoreClientSettings.Create($"esdb://localhost:{Container.GetMappedPublicPort(2113)}?tls=false");

    public EventStoreClient CreateClient() => new(ClientSettings);

    public async Task<IWriteResult> AppendEvents(string streamName, params EventData[] events)
    {
        var client = CreateClient();
        return await client.AppendToStreamAsync(streamName, StreamState.Any, events);
    }

    public EventStoreClient.ReadStreamResult ReadEvents(string streamName, bool resolveLinkTos = false)
        => CreateClient()
            .ReadStreamAsync(
                Direction.Forwards,
                streamName,
                global::EventStore.Client.StreamPosition.Start,
                resolveLinkTos: resolveLinkTos);

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