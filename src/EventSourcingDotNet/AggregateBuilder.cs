using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet;

public sealed class AggregateBuilder : IAggregateBuilder<AggregateBuilder>
{
    private readonly AggregateBuilder? _defaults;
    private ISnapshotProvider? _snapshotProvider;

    public AggregateBuilder(AggregateBuilder? defaults = null)
    {
        _defaults = defaults;
    }

    public ISnapshotProvider? SnapshotProvider => _snapshotProvider ?? _defaults?.SnapshotProvider;

    public AggregateBuilder UseSnapshotProvider(ISnapshotProvider provider)
    {
        _snapshotProvider = provider;
        return this;
    }

    internal void ConfigureServices(IServiceCollection services, Type aggregateIdType, IEnumerable<Type> stateTypes)
    {
        if (SnapshotProvider is { } snapshotProvider)
        {
            snapshotProvider.RegisterServices(services, aggregateIdType, stateTypes);
        }
    }
}