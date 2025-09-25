using System.Text;
using AwesomeAssertions;
using Newtonsoft.Json;
using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

[Collection(nameof(EventStoreCollection))]
public sealed class EventStoreTests(EventStoreFixture fixture)
{
    [Fact]
    public async Task ShouldReadEventsFromEventStore()
    {
        var aggregateId = new TestAggregateId();
        var @event = new TestEvent();

        var eventData = EventDataHelper.CreateEventData(aggregateId, @event);

        await fixture.AppendEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId), eventData);
        var eventStore = CreateEventStore();

        var result = await eventStore.ReadEventsAsync(aggregateId, default).ToListAsync();

#pragma warning disable CS8602
        result.Select(resolvedEvent => resolvedEvent.Event)
            .Should()
            .Contain(@event);
#pragma warning restore CS8602
    }

    [Fact]
    public async Task ShouldWriteEventsToEventStore()
    {
        var aggregateId = new TestAggregateId();
        var @event = new TestEvent();
        var eventStore = CreateEventStore();

        await eventStore.AppendEventsAsync(aggregateId, [@event], default);

        var appendedEvents = await fixture
            .ReadEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId))
            .ToListAsync();

        appendedEvents.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnCurrentAggregateVersionWhenAddingZeroEvents()
    {
        var aggregateId = new TestAggregateId();
        var eventStore = CreateEventStore();

        var result = await eventStore.AppendEventsAsync(aggregateId, [], default);

        result.Version.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnNextExpectedAggregateVersionWhenAddingEvents()
    {
        var aggregateId = new TestAggregateId();
        var eventStore = CreateEventStore();
        var events = Enumerable.Range(0, 5).Select(_ => new TestEvent()).ToList();

        var result = await eventStore.AppendEventsAsync(aggregateId, events, default);

        result.Version.Should().Be(5);
    }

    [Fact]
    public async Task ShouldReturnEmptyEnumerableWhenStreamDoesNotExist()
    {
        var aggregateId = new TestAggregateId();
        var eventStore = CreateEventStore();

        var result = await eventStore.ReadEventsAsync(aggregateId, default).ToListAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldWriteCorrelationIdInMetadata()
    {
        var aggregateId = new TestAggregateId();
        var correlationId = new CorrelationId(Guid.NewGuid());
        var @event = new TestEvent();

        var eventStore = CreateEventStore();

        await eventStore.AppendEventsAsync(aggregateId, [@event], default, correlationId: correlationId);

        var metadata = await ReadEventMetadata(aggregateId).FirstAsync();

        metadata?.CorrelationId.Should().Be(correlationId.Id);
    }

    [Fact]
    public async Task ShouldReadCorrelationId()
    {
        var aggregateId = new TestAggregateId();
        var @event = new TestEvent();
        var correlationId = new CorrelationId();

        var eventData = EventDataHelper.CreateEventData(
            aggregateId,
            @event,
            correlationId: correlationId);

        await fixture.AppendEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId), eventData);
        var eventStore = CreateEventStore();

        var result = await eventStore
            .ReadEventsAsync(aggregateId, default)
            .Select(x => x.CorrelationId)
            .ToListAsync();

        result.Should().Contain(correlationId);
    }

    [Fact]
    public async Task ShouldWriteCausationIdInMetadata()
    {
        var aggregateId = new TestAggregateId();
        var causationId = new CausationId(Guid.NewGuid());
        var @event = new TestEvent();

        var eventStore = CreateEventStore();

        await eventStore.AppendEventsAsync(aggregateId, [@event], default, causationId: causationId);

        var metadata = await ReadEventMetadata(aggregateId).FirstAsync();

        metadata?.CausationId.Should().Be(causationId.Id);
    }

    private EventStore<TestAggregateId> CreateEventStore(IEventSerializer? eventSerializer = null)
        => new(
            eventSerializer ?? new EventSerializer(TestJsonSerializerContext.Default),
            new(fixture.ClientSettings));

    private IAsyncEnumerable<EventMetadata<TestAggregateId>?> ReadEventMetadata(TestAggregateId aggregateId)
        => fixture
            .ReadEvents(StreamNamingConvention.GetAggregateStreamName(aggregateId))
            .Select(resolvedEvent => JsonConvert.DeserializeObject<EventMetadata<TestAggregateId>>(
                Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span)));
}