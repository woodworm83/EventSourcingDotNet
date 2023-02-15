using EventSourcingDotNet.InMemory;

// ReSharper disable once CheckNamespace
namespace EventSourcingDotNet;

public static class InMemoryRegistrationExtensions
{
    public static EventSourcingBuilder UseInMemoryEventStore(this EventSourcingBuilder builder)
        => builder.UseEventStoreProvider(new InMemoryEventStoreProvider());

    public static TBuilder UseInMemorySnapshot<TBuilder>(this TBuilder builder)
        where TBuilder : IAggregateBuilder<TBuilder>
        => builder.UseSnapshotProvider(new InMemorySnapshotProvider());
}