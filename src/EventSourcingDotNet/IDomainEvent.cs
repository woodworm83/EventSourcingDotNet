using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace EventSourcingDotNet;

public interface IDomainEvent
{
}

// ReSharper disable once UnusedTypeParameter
[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed")]
public interface IDomainEvent<TAggregateId> : IDomainEvent
    where TAggregateId : IAggregateId
{
}

public interface IDomainEvent<TAggregateId, TState> : IDomainEvent<TAggregateId>
    where TAggregateId : IAggregateId
{
    TState Apply(TState state);

    EventValidationResult Validate(TState state) => EventValidationResult.Fire;
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