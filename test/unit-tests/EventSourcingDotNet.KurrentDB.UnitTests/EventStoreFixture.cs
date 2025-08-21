using DotNet.Testcontainers.Containers;
using KurrentDB.Client;
using Testcontainers.EventStoreDb;
using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed class EventStoreFixture : IAsyncLifetime
{
    public IContainer Container { get; } = new EventStoreDbBuilder().Build();
    
    public KurrentDBClientSettings ClientSettings
        => KurrentDBClientSettings.Create($"esdb://localhost:{Container.GetMappedPublicPort(2113)}?tls=false");

    public KurrentDBClient CreateClient() => new(ClientSettings);

    public async Task<IWriteResult> AppendEvents(string streamName, params EventData[] events)
    {
        var client = CreateClient();
        return await client.AppendToStreamAsync(streamName, StreamState.Any, events).ConfigureAwait(false);
    }

    public KurrentDBClient.ReadStreamResult ReadEvents(string streamName, bool resolveLinkTos = false)
        => CreateClient()
            .ReadStreamAsync(
                Direction.Forwards,
                streamName,
                global::KurrentDB.Client.StreamPosition.Start,
                resolveLinkTos: resolveLinkTos);

    public async Task InitializeAsync()
    {
        await Container.StartAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync().ConfigureAwait(false);
    }
}