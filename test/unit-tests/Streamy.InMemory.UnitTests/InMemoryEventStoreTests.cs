using FluentAssertions;
using Xunit;

namespace Streamy.InMemory.UnitTests;

public class InMemoryEventStoreTests
{
    [Fact]
    public async Task ShouldReturnAppendedEvents()
    {
        var @event = new TestEvent();
        var eventStore = new InMemoryEventStore<TestId>();
        await eventStore.AppendEventsAsync(default, new[] {@event}, default);

        var events = await eventStore.ReadEventsAsync(default, default)
            .Select(x => x.Event)
            .ToListAsync();
            
        events
            .Should()
            .Equal(@event);
    }

    private readonly record struct TestId : IAggregateId
    {
        public static string AggregateName => "test";

        public string AsString() => string.Empty;
    }

    private sealed record TestEvent : IDomainEvent<object>
    {
        public object Apply(object state) => state;
    }
}