EventSourcingDotNet
===================

***Event Sourcing made easy.***

A storage agnostic library to implement event sourced systems.

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
  To create a new instance of the aggregate the aggregate repository and the snapshot providers must be able to create a new instance of the state record so that it can rely on a defined initial state prior to replay the events.

## Define your events

Create a record for each and every event happening on your aggregates.
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

## Update Aggregate State

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

## Configuration using Dependency Injection
The library uses [Microsoft Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection).
It provides a set of extension methods and builders to allow fluent dependency injection configuration.

### Single Aggregate Registration

#### Register an aggregate using In-Memory event storage provider:
You must add a reference to [EventSourcingDotNet.InMemory](https://www.nuget.org/packages/EventSourcingDotNet.InMemory) package.

    services.AddEventSourcing(builder => 
    {
        builder.AddAggregate<MyAggregateId, MyAggregateState>()
            .UseInMemoryEventStore();
    }

#### Register an aggregate using EventStoreDB storage provider:
You must add a reference to [EventSourcingDotNet.EventStore](https://www.nuget.org/packages/EventSourcingDotNet.EventStore) package.

    sevrices.AddEventSourcing(builder =>
    {
        builder.AddAggregate<MyAggregateId, MyAggregateState>()
            .UseEventStore();
    }

    services.ConfigureEventStore(new Uri("esdb://localhost:2113"));

All aggregates use the same event store configuration. At this time it is not possible to configure different configurations for individual aggregates.

### Aggregate Registration using Assembly Scanning
It is possible to register all aggregates in one or more assemblies with the same provider configuration.\
You can use the same aggregate builder methods to configure storage providers as described above.

There are two Builder methods to collect the aggregate types using assembly scanning:
* `Scan(params Type[] assemblyMarkerTypes)`
* `Scan(params Assembly[] assemblies)`

#### Example code to configure In-Memory Storage provider for all types found using assembly scanning:

    services.AddEventSourcing(builder => 
    {
        builder.Scan(typeof(TypeInAssembly))
            .UseInMemoryEventStore()
    }

It is possible to scan the assembly for aggregate types and then override the configuration of individual aggregates.

    services.AddEventSourcing(builder =>
    {
        builder.Scan(typeof(TypeInAssembly))
            .UseEventStore();
        builder.AddAggregate<MyAggregateId, MyAggregateType>()
            .UseInMemoryEventStore();
    }

### Snapshot provider configuration
You can specify a snapshot provider for each single-aggregate or assembly scanning registration.
Currently there is only an In-Memory snapshot provider available.

    services.AddEventSourcing(builder => 
    {
        builder.AddAggregate<MyAggregateId, MyAggregateState>()
            .UseEventStore()
            .UseInMemorySnapshotProvider();
    }

Be careful when using the In-Memory snapshot provider. It keeps the current state of every aggregate in memory.
Use it to avoid a full replay of all events when loading the aggregate from the repository when write performance is critical.
It should not be used if there are many aggregates of the same type.

As shown in the example above it is possible to use a mix of event storage and snapshot providers.

# Supported Data Stores
### Event Storage Providers
* In-Memory
* [Event Store DB](https://www.eventstore.com/) (experimental)

### Snapshot Providers
* In-Memory

#### Planned Snapshot Providers
* Redis
* A variety of No-SQL Document Databases 
