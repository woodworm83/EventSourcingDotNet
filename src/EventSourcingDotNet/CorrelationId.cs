namespace EventSourcingDotNet;

public readonly record struct CorrelationId(Guid Id)
{
    public CorrelationId()
        : this(Guid.NewGuid())
    { }
}