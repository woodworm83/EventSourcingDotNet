namespace EventSourcingDotNet;

public interface IAggregateBuilder<out TBuilder>
    where TBuilder : IAggregateBuilder<TBuilder>
{
    TBuilder UseSnapshotProvider(ISnapshotProvider provider);
}