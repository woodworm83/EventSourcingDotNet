namespace EventSourcingDotNet;

public interface IAggregateBuilder<out TBuilder>
    where TBuilder : IAggregateBuilder<TBuilder>
{
    public TBuilder UseSnapshotProvider(ISnapshotProvider provider);
}