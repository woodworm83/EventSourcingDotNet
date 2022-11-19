using EventSourcingDotNet.InMemory;

// ReSharper disable once CheckNamespace
namespace EventSourcingDotNet;

public static class RegistrationExtensions
{
    public static AggregateBuilder UseInMemoryEventStore(
        this AggregateBuilder builder)
        => builder.UseEventStoreProvider(new InMemoryEventStoreProvider());
}