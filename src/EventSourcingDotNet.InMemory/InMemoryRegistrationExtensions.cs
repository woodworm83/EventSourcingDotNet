using EventSourcingDotNet.InMemory;

// ReSharper disable once CheckNamespace
namespace EventSourcingDotNet;

public static class InMemoryRegistrationExtensions
{
    public static AggregateBuilder UseInMemoryEventStore(
        this AggregateBuilder builder)
        => builder.UseEventStoreProvider(new InMemoryEventStoreProvider());

    public static AggregateBuilder UseInMemorySnapshot(
        this AggregateBuilder builder)
        => builder.UseSnapshotProvider(new InMemorySnapshotProvider());
}