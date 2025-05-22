namespace EventSourcingDotNet;

public sealed class OptimisticConcurrencyException : Exception
{
    public OptimisticConcurrencyException(AggregateVersion expectedVersion, AggregateVersion actualVersion)
        : base($"Version {actualVersion} does not match expected version {expectedVersion}")
    {
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public AggregateVersion ExpectedVersion { get; }
    public AggregateVersion ActualVersion { get; }
}