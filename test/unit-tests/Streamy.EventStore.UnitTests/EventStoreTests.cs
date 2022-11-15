using System.Text;
using EventStore.Client;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Xunit;

namespace Streamy.EventStore.UnitTests;

[Collection(nameof(EventStoreCollection))]
public class EventStoreTests
{
    private static readonly EventTypeResolver<TestId> _eventTypeResolver = new();
    
    private readonly EventStoreTestContainer _container;
    private readonly EventSerializer<TestId> _eventSerializer = new(_eventTypeResolver);
    private readonly StreamNamingConvention<TestId> _streamNamingConvention = new();

    public EventStoreTests(EventStoreFixture fixture)
    {
        _container = fixture.Container;
    }

    [Fact]
    public async Task ShouldReadEventsFromEventStore()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent(42);
        var eventData = CreateEventData(aggregateId, @event);
        await _container.AppendEvents(_streamNamingConvention.GetAggregateStreamName(aggregateId), eventData);
        var eventStore = CreateEventStore();

        var result = await eventStore.ReadEventsAsync(aggregateId, default).ToListAsync();

        result.Should().Equal(new[] {@event}, (resolvedEvent, e) => resolvedEvent.Event.Equals(e));
    }

    [Fact]
    public async Task ShouldWriteEventsToEventStore()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent(42);
        var eventStore = CreateEventStore();

        await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default);

        var appendedEvents = await _container.ReadEvents(_streamNamingConvention.GetAggregateStreamName(aggregateId)).ToListAsync();

        appendedEvents.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnCurrentAggregateVersionWhenAddingZeroEvents()
    {
        var aggregateId = new TestId();
        var eventStore = CreateEventStore();

        var result = await eventStore.AppendEventsAsync(aggregateId, Array.Empty<IDomainEvent<TestId>>(), default);

        result.Version.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnNextExpectedAggregateVersionWhenAddingEvents()
    {
        var aggregateId = new TestId();
        var eventStore = CreateEventStore();
        var events = Enumerable.Range(0, 5).Select(i => new TestEvent(i)).ToList();

        var result = await eventStore.AppendEventsAsync(aggregateId, events, default);

        result.Version.Should().Be(5);
    }

    private static EventData CreateEventData(TestId aggregateId, TestEvent? @event = null, Guid? eventId = null)
        => new(
            eventId is null ? Uuid.NewUuid() : Uuid.FromGuid(eventId.Value),
            nameof(TestEvent),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event ?? new TestEvent())),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EventMetadata<TestId>(aggregateId))));

    private EventStore<TestId> CreateEventStore() 
        => new(
            Options.Create(_container.ClientSettings),
            _eventSerializer,
            _streamNamingConvention);
}