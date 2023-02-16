using System.Runtime.Serialization;

namespace EventSourcingDotNet;

public interface IDomainEvent
{
}

[Serializable]
public sealed class EventValidationException : ApplicationException
{
    public EventValidationException(string message)
        : base($"Event validation failed:\n{message}")
    {
    }

    private EventValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

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