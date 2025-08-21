namespace EventSourcingDotNet;

public interface IAggregateId
{
    static abstract string AggregateName { get; }

    string? AsString();
}