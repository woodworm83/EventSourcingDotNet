using System.Reactive.Linq;
using System.Reactive.Subjects;
using EventSourcingDotNet.Serialization.Json;
using EventStore.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

[Collection(nameof(EventStoreCollection))]
public class EventListenerTests
{
    private readonly EventStoreFixture _fixture;

    public EventListenerTests(EventStoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ShouldNotifyEventsByAggregateId()
    {
        var aggregateId = new AggregateId();
        await using var publisher = CreateEventListener();
        using var receiver = new ReplaySubject<ResolvedEvent>();
        var @event = new TestEvent();

        using (publisher.ByAggregate(aggregateId).Subscribe(receiver))
        {
            await _fixture.AppendEvents(
                StreamNamingConvention.GetAggregateStreamName(aggregateId),
                EventDataHelper.CreateEventData(aggregateId, @event));

            var result = await WaitForEvents(receiver, 1);

            result.Select(x => x.Event)
                .Should().Equal(@event);
        }
    }

    [Fact]
    public async Task ShouldNotifyEventsByCategory()
    {
        await using var publisher = CreateEventListener();
        using var receiverSubject = new ReplaySubject<ResolvedEvent>();
        var events = Enumerable.Range(0, 5).Select(_ => new TestEvent()).ToList();

        using (publisher.ByCategory<ByCategoryId>().Subscribe(receiverSubject))
        {
            foreach (var @event in events)
            {
                var aggregateId = new ByCategoryId();
                await _fixture.AppendEvents(
                    StreamNamingConvention.GetAggregateStreamName(aggregateId),
                    EventDataHelper.CreateEventData(aggregateId, @event));
            }

            var result = await WaitForEvents(receiverSubject, 5);

            result.Select(x => x.Event)
                .Should().Equal(events);
        }
    }

    [Fact]
    public async Task ShouldNotifyEventsByEventType()
    {
        await using var publisher = CreateEventListener();
        using var receiverSubject = new ReplaySubject<ResolvedEvent>();
        var events = Enumerable.Range(0, 5).Select(_ => new ByTypeEvent()).ToList();

        using (publisher.ByEventType<ByTypeEvent>().Subscribe(receiverSubject))
        {
            foreach (var @event in events)
            {
                var aggregateId = new ByEventTypeId();
                await _fixture.AppendEvents(
                    StreamNamingConvention.GetAggregateStreamName(aggregateId),
                    EventDataHelper.CreateEventData(aggregateId, @event),
                    EventDataHelper.CreateEventData(aggregateId, new TestEvent()));
            }

            var result = await WaitForEvents(receiverSubject, 5);

             result.Select(x => x.Event)
                .Should().Equal(events);
        }
    }

    private EventListener CreateEventListener() 
        => new(
            new EventSerializer(
                new TestEventTypeResolver(),
                new JsonSerializerSettingsFactory(NullLoggerFactory.Instance)),
            new EventStoreClient(Options.Create(_fixture.ClientSettings)));

    private static async Task<IReadOnlyList<ResolvedEvent>> WaitForEvents(
        IObservable<ResolvedEvent> source,
        int count)
        => await source
            .Take(count)
            .Timeout(TimeSpan.FromSeconds(2))
            .ToAsyncEnumerable()
            .ToListAsync();

    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly record struct AggregateId(Guid Id) : IAggregateId
    {
        public AggregateId() : this(Guid.NewGuid())
        {
        }

        public static string AggregateName => "aggregate";
        public string AsString() => Id.ToString();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly record struct ByCategoryId(Guid Id) : IAggregateId
    {
        public ByCategoryId() : this(Guid.NewGuid())
        {
        }

        public static string AggregateName => "category";

        public string AsString() => Id.ToString();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly record struct ByEventTypeId(Guid Id) : IAggregateId
    {
        public ByEventTypeId() : this(Guid.NewGuid())
        {
        }

        public static string AggregateName => "eventType";

        public string AsString() => Id.ToString();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly record struct ByCorrelationId(Guid Id) : IAggregateId
    {
        public ByCorrelationId() : this(Guid.NewGuid())
        {
        }

        public static string AggregateName => "correlation";

        public string AsString() => Id.ToString();
    }
}