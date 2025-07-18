using System.Text;
using EventSourcingDotNet.Serialization.Json;
using FluentAssertions;
using KurrentDB.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

[Collection(nameof(EventStoreCollection))]
public class EventStoreTests
{
    private static readonly TestEventTypeResolver EventTypeResolver = new();
    
    private readonly EventStoreFixture _fixture;
    private readonly JsonSerializerSettingsFactory _serializerSettingsFactory = new(NullLoggerFactory.Instance);

    public EventStoreTests(EventStoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ShouldReadEventsFromEventStore()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent(42);
        var eventData = EventDataHelper.CreateEventData(aggregateId, @event);
        await _fixture.AppendEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId), eventData);
        var eventStore = CreateEventStore();

        var result = await eventStore.ReadEventsAsync(aggregateId, default).ToListAsync();

#pragma warning disable CS8602
        result.Should().Equal(new[] {@event}, (resolvedEvent, e) => resolvedEvent.Event.Equals(e));
#pragma warning restore CS8602
    }

    [Fact]
    public async Task ShouldWriteEventsToEventStore()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent(42);
        var eventStore = CreateEventStore();

        await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default);

        var appendedEvents = await _fixture.ReadEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId))
            .ToListAsync();

        appendedEvents.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnCurrentAggregateVersionWhenAddingZeroEvents()
    {
        var aggregateId = new TestId();
        var eventStore = CreateEventStore();

        var result = await eventStore.AppendEventsAsync(aggregateId, Array.Empty<IDomainEvent>(), default);

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

    [Fact]
    public async Task ShouldReturnEmptyEnumerableWhenStreamDoesNotExist()
    {
        var aggregateId = new TestId();
        var eventStore = CreateEventStore();

        var result = await eventStore.ReadEventsAsync(aggregateId, default).ToListAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldWriteCorrelationIdInMetadata()
    {
        var aggregateId = new TestId();
        var correlationId = new CorrelationId(Guid.NewGuid());
        var @event = new TestEvent();

        var eventStore = CreateEventStore();

        await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default, correlationId: correlationId);

        var metadata = await ReadEventMetadata(aggregateId).FirstAsync();
            
        metadata?.CorrelationId.Should().Be(correlationId.Id);
    }

    [Fact]
    public async Task ShouldReadCorrelationId()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent(42);
        var correlationId = new CorrelationId();
        var eventData = EventDataHelper.CreateEventData(aggregateId, @event, correlationId: correlationId);
        await _fixture.AppendEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId), eventData);
        var eventStore = CreateEventStore();

        var result = await eventStore.ReadEventsAsync(aggregateId, default)
            .Select(x => x.CorrelationId)
            .ToListAsync();

        result.Should().Equal(correlationId);
    }

    [Fact]
    public async Task ShouldWriteCausationIdInMetadata()
    {
        var aggregateId = new TestId();
        var causationId = new CausationId(Guid.NewGuid());
        var @event = new TestEvent();

        var eventStore = CreateEventStore();

        await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default, causationId: causationId);

        var metadata = await ReadEventMetadata(aggregateId).FirstAsync();
            
        metadata?.CausationId.Should().Be(causationId.Id);
    }

    private EventStore<TestId> CreateEventStore(IEventSerializer? eventSerializer = null)
        => new(
            eventSerializer ?? new EventSerializer(EventTypeResolver, _serializerSettingsFactory),
            new KurrentDBClient(_fixture.ClientSettings));

    private IAsyncEnumerable<EventMetadata?> ReadEventMetadata(TestId aggregateId) 
        => _fixture.ReadEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId))
            .Select(
                resolvedEvent => JsonConvert.DeserializeObject<EventMetadata>(
                    Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span)));
}