using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace EventSourcingDotNet;

[Serializable]
public sealed class OptimisticConcurrencyException : Exception
{
    public OptimisticConcurrencyException(AggregateVersion expectedVersion, AggregateVersion actualVersion)
        : base($"Version {actualVersion} does not match expected version {expectedVersion}")
    {
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    [ExcludeFromCodeCoverage]
    private OptimisticConcurrencyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ExpectedVersion = new AggregateVersion(info.GetUInt64(nameof(ExpectedVersion)));
        ActualVersion = new AggregateVersion(info.GetUInt64(nameof(ActualVersion)));
    }

    [ExcludeFromCodeCoverage]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ExpectedVersion), ExpectedVersion.Version);
        info.AddValue(nameof(ActualVersion), ActualVersion.Version);
    }

    public AggregateVersion ExpectedVersion { get; }
    public AggregateVersion ActualVersion { get; }
}