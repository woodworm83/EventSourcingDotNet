using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

[Collection(nameof(EventStoreCollection))]
public class EventStoreTests
{
    private static readonly EventTypeResolver<TestId> _eventTypeResolver = new();

    private readonly EventStoreTestContainer _container;
    private readonly EventSerializer<TestId> _eventSerializer = new(_eventTypeResolver);

    public EventStoreTests(EventStoreFixture fixture)
    {
        _container = fixture.Container;
    }

    [Fact]
    public async Task ShouldReadEventsFromEventStore()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent(42);
        var eventData = EventDataHelper.CreateEventData(aggregateId, @event);
        await _container.AppendEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId), eventData);
        await using var eventStore = CreateEventStore();

        var result = await eventStore.ReadEventsAsync(aggregateId, default).ToListAsync();

        result.Should().Equal(new[] {@event}, (resolvedEvent, e) => resolvedEvent.Event.Equals(e));
    }

    [Fact]
    public async Task ShouldWriteEventsToEventStore()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent(42);
        await using var eventStore = CreateEventStore();

        await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default);

        var appendedEvents = await _container.ReadEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId))
            .ToListAsync();

        appendedEvents.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnCurrentAggregateVersionWhenAddingZeroEvents()
    {
        var aggregateId = new TestId();
        await using var eventStore = CreateEventStore();

        var result = await eventStore.AppendEventsAsync(aggregateId, Array.Empty<IDomainEvent<TestId>>(), default);

        result.Version.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnNextExpectedAggregateVersionWhenAddingEvents()
    {
        var aggregateId = new TestId();
        await using var eventStore = CreateEventStore();
        var events = Enumerable.Range(0, 5).Select(i => new TestEvent(i)).ToList();

        var result = await eventStore.AppendEventsAsync(aggregateId, events, default);

        result.Version.Should().Be(5);
    }

    private EventStore<TestId> CreateEventStore()
        => new(
            Options.Create(_container.ClientSettings),
            _eventSerializer);
}