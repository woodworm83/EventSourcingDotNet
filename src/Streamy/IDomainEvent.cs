namespace Streamy;

public interface IDomainEvent { }

public interface IDomainEvent<TState> : IDomainEvent
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