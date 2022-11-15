namespace Streamy;

public interface IDomainEvent<TAggregateId> 
    where TAggregateId : IAggregateId
{ }

public interface IDomainEvent<TAggregateId, TState> : IDomainEvent<TAggregateId>
    where TAggregateId : IAggregateId
{
    TState Apply(TState state);

    EventValidationResult Validate(TState state) => EventValidationResult.Fire;
}

public abstract record EventValidationResult
{
    private EventValidationResult() { }

    public static EventValidationResult Fire => new Fired();

    public static EventValidationResult Skip => new Skipped();

    public static EventValidationResult Fail(Exception exception) => new Failed(exception);

    internal sealed record Fired : EventValidationResult;

    internal sealed record Skipped : EventValidationResult;

    internal sealed record Failed(Exception Exception) : EventValidationResult;
}