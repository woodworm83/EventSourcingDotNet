# EventSourcingDotNet
Event Sourcing made easy

## Event Store Providers

* In-Memory
* [Event Store DB](https://www.eventstore.com/) (experimental)

## Snapshot Providers

* In-Memory

# Getting Started

## Define your aggregate state and ID

### Aggregate ID

Create a readonly ID record struct for each of your aggregates.

It must implement the `IAggregateId` interface to be accepted as an aggregate ID.

The `static AggregateName` property and the `AsString()` method are used to compose the stream name. You should not use dashes in your `AggregateName` because some providers, e.g. [Event Store DB](https://www.eventstore.com/), will split the stream name at the first dash to provide a by-category stream.  

    public readonly record struct MyAggregateId(Guid Id) : IAggregateId
    {
        public static string AggregateName => "myAggregate";

        public string AsString() => Id.ToString();
    }

### Aggregate State

Create a state record representing the current state of your aggregate.

It must implement the generic `IAggregateState<TAggregatId>` interface to be accepted as an aggregate state.
The generic type argument `TAggregateId` is the aggregate ID specified above.

    public sealed record MyAggregate : IAggregateState<MyAggregateId>
    {
        public int MyValue { get; init; }
    }

#### Aggregate State Rules
* **The state record should be immutable.**\
  Some storage or snapshot providers, e.g. the In-Memory snapshot provider, may keep a reference to the latest version of the aggregate. Mutations of the aggregate state may lead to an inconsistent aggregate state.


* **The state record must provide a public parameterless constructor.**\
  To create a new instance of the

## Define your events

Create a record for each and every event happening on your aggregate.
An event must implement the `IDomainEvent<TAggregateId, TState>` interface to be accepted as a valid event.

Each event must implement the `Apply(TState)` method to update the aggregate state.

    public sealed record SomethingHappened : IDomainEvent<SomeState>
    {
        public Something Apply(SomeState state)
        {
            // return updated state here
        }
    }

### Event validation
The `IDomainEvent<TAggregateId, TState>` interface provides an additional `Validate` method allowing to implement logic whether an event should be fired, skipped or raise an error.
The validation happens before the event is applied to the aggregate state and added to the uncommitted events.

    public sealed record SomethingHappened : IDomainEvent<SomeState>
    {
        // Apply method removed for brevity

        public EventValidationResult Validate(SomeState state)
        {
            // do some validation logic here...
            return EventValidationResult.Fire;
        }
    }

You can return the following validation results:

* `EventValidationResult.Fire`\
The event will be applied to the aggregate state and added to the uncommitted events collection.


* `EventValidationResult.Skip`\
The event will be skipped. It will not be applied to the aggregate state and not added to the uncommitted events collection.\
Use this validation result when the aggregate state will not be affected by the event and you want to avoid extra effect-less events in the event stream


* `EventValidationResult.Fail(Exception)`\
The event cannot be applied in the current state of the aggregate or it would lead to an inconsistent state.\
The method takes an Exception parameter which expresses why this event cannot be applied.

## Define update logic

Aggregates are updated by getting the current version from the aggregate repository, 
adding some new events and then saving the aggregate back to the event store.

Avoid to run long running tasks after getting the aggregate from the repository and storing it back as it increases the risk of running into a concurrency issue.\
All Event Store providers use optimistic concurrency checks to avoid update conflicts.

    private readonly IAggregateRepository<SomeId, SomeState> _repository;

    public async Task DoSomething(SomeRequest request)
    {
        var aggregate = await _repository.GetByIdAsync(request.Id);

        aggregate = aggregate.AddEvent(new SomethingHappened());

        await _repository.SaveAsync(aggregate);
    }

Alternatively there is a shorthand extension method allowing to retrieve, update and save the aggregate in one statement:
    
    IAggregateRepository<TAggregateId, TState>.UpdateAsync(TAggregateId aggregateId, params IDomainEvent<TState>[] events);
        
