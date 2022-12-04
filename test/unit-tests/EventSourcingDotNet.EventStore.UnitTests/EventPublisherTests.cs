using System.Reactive.Linq;
using System.Reactive.Subjects;
using EventSourcingDotNet.Serialization.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

[Collection(nameof(EventStoreCollection))]
public class EventPublisherTests
{
    private readonly EventStoreTestContainer _container;

    public EventPublisherTests(EventStoreFixture fixture)
    {
        _container = fixture.Container;
    }

    [Fact]
    public async Task ShouldNotifyEventsByAggregateId()
    {
        var aggregateId = new AggregateId();
        await using var publisher = CreateEventPublisher<AggregateId>();
        using var receiver = new ReplaySubject<ResolvedEvent<AggregateId>>();
        var @event = new TestEvent();

        using (publisher.Listen(aggregateId).Subscribe(receiver))
        {
            await _container.AppendEvents(
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
        await using var publisher = CreateEventPublisher<ByCategoryId>();
        using var receiverSubject = new ReplaySubject<ResolvedEvent<ByCategoryId>>();
        var events = Enumerable.Range(0, 5).Select(_ => new TestEvent()).ToList();

        using (publisher.Listen().Subscribe(receiverSubject))
        {
            foreach (var @event in events)
            {
                var aggregateId = new ByCategoryId();
                await _container.AppendEvents(
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
        await using var publisher = CreateEventPublisher<ByEventTypeId>();
        using var receiverSubject = new ReplaySubject<ResolvedEvent<ByEventTypeId>>();
        var events = Enumerable.Range(0, 5).Select(_ => new TestEvent()).ToList();

        using (publisher.Listen<TestEvent>().Subscribe(receiverSubject))
        {
            foreach (var @event in events)
            {
                var aggregateId = new ByEventTypeId();
                await _container.AppendEvents(
                    StreamNamingConvention.GetAggregateStreamName(aggregateId),
                    EventDataHelper.CreateEventData(aggregateId, @event),
                    EventDataHelper.CreateEventData(aggregateId, new OtherEvent()));
            }

            var result = await WaitForEvents(receiverSubject, 5);

             result.Select(x => x.Event)
                .Should().Equal(events);
        }
    }

    private EventListener<TAggregateId> CreateEventPublisher<TAggregateId>()
        where TAggregateId : IAggregateId
        => new(
            Options.Create(_container.ClientSettings),
            new EventSerializer<TAggregateId>(
                new EventTypeResolver<TAggregateId>(),
                new JsonSerializerSettingsFactory<TAggregateId>(NullLoggerFactory.Instance)));

    private static async Task<IReadOnlyList<ResolvedEvent<TAggregateId>>> WaitForEvents<TAggregateId>(
        IObservable<ResolvedEvent<TAggregateId>> source,
        int count)
        where TAggregateId : IAggregateId
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

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record TestEvent(Guid Id)
        : IDomainEvent<AggregateId>, IDomainEvent<ByCategoryId>, IDomainEvent<ByEventTypeId>
    {
        public TestEvent() : this(Guid.NewGuid())
        {
        }
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record OtherEvent(Guid Id)
        : IDomainEvent<ByEventTypeId>
    {
        public OtherEvent() : this(Guid.NewGuid())
        {
        }
    }
}