namespace EventSourcingDotNet;

public readonly record struct AggregateVersion(ulong Version)
{
    public static AggregateVersion operator ++(AggregateVersion version)
        => new(version.Version + 1);
}