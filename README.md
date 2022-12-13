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
        public SomeState Apply(SomeState state)
        {
            // return updated state here
        }
    }

### Event validation
The `IDomainEvent<TAggregateId, TState>` interface provides an additional `Validate` method allowing to implement logic whether an event should be fired, skipped or raise an error.
The validation happens before the event is applied to the aggregate state and added to the uncommitted events.
The default implementation always returns `EventValidationResult.Fire`.

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
You can configure providers globally and/or by aggregate.

### Single Aggregate Registration

#### Register an aggregate using In-Memory event storage provider:
You must add a reference to [EventSourcingDotNet.InMemory](https://www.nuget.org/packages/EventSourcingDotNet.InMemory) package.

Provider Configuration by Aggregate:

    services.AddEventSourcing(builder => 
    {
        builder.AddAggregate<MyAggregateId, MyAggregateState>()
            .UseInMemoryEventStore();
    }

Global Provider Configuration:
    
    services.AddEventSourcing(builder => 
    {
        builder.UseInMemoryEventStore();
        builder.AddAggregate<MyAggregateId, MyAggregateState>();
        builder.AddAggregate<MyOtherAggregateId, MyOtherAggregateState>();
    }

#### Register an aggregate using EventStoreDB storage provider:
You must add a reference to [EventSourcingDotNet.EventStore](https://www.nuget.org/packages/EventSourcingDotNet.EventStore) package.

Provider Configuration by Aggregate:

    sevrices.AddEventSourcing(builder =>
    {
        builder.AddAggregate<MyAggregateId, MyAggregateState>()
            .UseEventStore("esdb://localhost:2113");
    }

Global Provider Configuration:

    sevrices.AddEventSourcing(builder =>
    {
        builder.UseEventStore("esdb://localhost:2113");
        builder.AddAggregate<MyAggregateId, MyAggregateState>();
    }

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

# Crypto Shredding

The Library has built-in support for crypto shredding.

Event property encryption requires an implementation of an `IEncryptionKeyStore` to store the encryption keys and a `ICryptoProvider` to provide a symmetric encryption.
By default, the built-in crypto provider `AesCryptoProvider` is used to encrypt the values using AES algorithm and PKCS7 padding.

To encrypt a property in an event, it can be marked with an `EncryptAttribute`.

    public sealed record MyEvent(
        [property: Encrypt] string MyValue)
        : IDomainEvent<MyAggregateId, MyAggregateState>;

Or

    public sealed record MyEvent: IDomainEvent<MyAggregateId, MyAggregateState>
    {
        [Encrypt]
        public string MyValue { get; init; }
    }

Encrypted property names will be prefixed with a # to indicate an encrypted value.
The value is encoded using Base64 encoding.

The event `MyEvent` shown above will be serialized as:

    {
      "#myValue": "eyJpdiI6IjJ3YXE3OFRGTTRjNkovQXUvVHdDZWc9PSIsImN5cGhlciI6ImpKOW5jaXlNTkQ1WG9wanR1b3Qxc0E9PSJ9"
    }

To use the file based encryption key provider register it using extenstion method:

    IServiceCollection AddFileEncryptionKeyStore(
        this IServiceCollection services, 
        IConfigurationSection configurationSection);

# Supported Data Stores
### Event Storage Providers
* In-Memory
* [Event Store DB](https://www.eventstore.com/)

### Snapshot Providers
* In-Memory

#### Planned Snapshot Providers
* Redis
* A variety of No-SQL Document Databases 

### Encryption Key Store Providers
* File Storage
