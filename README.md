# EventSourcingDotNet
Event Sourcing made easy

## Available Event Store Providers

* In-Memory

## Experimental Event Store Providers

* [Event Store DB](https://www.eventstore.com/)

# Getting Started

## Define your events

    public sealed record SomethingHappened : IDomainEvent<SomeState>
    {
        public Something Apply(SomeState state)
        {
            // return updated state here
        }
    }

### Event validation
The `IDomainEvent<TState>` interface provides an additional member allowing to implement logic whether an event should be fired, skipped or raise an error.
The validation happens before the event is written to the event store.

    public sealed record SomethingHappened : IDomainEvent<SomeState>
    {
        // Apply method removed for brevity

        public EventValidationResult Validate(SomeState state)
        {
            // the event should be fired
            return EventValidationResult.Fire;

            // the event should be skipped
            return EventValidationResult.Skip;

            // the event is not valid in this state
            return EventValidationResult.Fail(new AnExceptionTellingThatThisIsNotValid());
        }
    }

## Define your aggregate state

Aggregate state records should not contain any logic. They're just representing the current state of the aggregate.
Apply and validation logic are handled by the events.

    public sealed record SomeState(int SomeValue);

## Define update logic

    private readonly IAggregateRepository<SomeId, SomeState> _repository;

    public async Task DoSomething(SomeRequest request)
    {
        var aggregate = await _repository.GetByIdAsync(request.Id);

        aggregate = aggregate.AddEvent(new SomethingHappened());

        await _repository.SaveAsync(aggregate);
    }

Alternatively there is a shorthand extension method allowing to retrieve, update and save the aggregate in one statement:
    
    IAggregateRepository<TAggregateId, TState>.UpdateAsync(TAggregateId aggregateId, params IDomainEvent<TState>[] events);
        
