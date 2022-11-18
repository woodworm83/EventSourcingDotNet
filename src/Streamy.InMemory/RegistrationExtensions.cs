namespace Streamy.InMemory;

public static class RegistrationExtensions
{
    public static AggregateBuilder UseInMemoryEventStore(
        this AggregateBuilder builder)
        => builder.UseEventStoreProvider(new InMemoryEventStoreProvider());
}