namespace EventSourcingDotNet;

public interface IAggregateId
{
    public static abstract string AggregateName { get; }

    public string? AsString();
}