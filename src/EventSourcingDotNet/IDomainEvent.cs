namespace EventSourcingDotNet;

public interface IDomainEvent
{
}

public sealed class EventValidationException(string message) : Exception($"Event validation failed:\n{message}");

public abstract record EventValidationResult
{
    private EventValidationResult()
    {
    }

    public static EventValidationResult Fire => new Fired();

    public static EventValidationResult Skip => new Skipped();

    public static EventValidationResult Fail(string message) => new Failed(message);

    internal sealed record Fired : EventValidationResult;

    internal sealed record Skipped : EventValidationResult;

    internal sealed record Failed(string Message) : EventValidationResult;
}