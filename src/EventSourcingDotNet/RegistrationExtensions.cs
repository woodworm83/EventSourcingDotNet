using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet;

public static class RegistrationExtensions
{
    public delegate void ConfigureEventSourcing(EventSourcingBuilder builder);
    
    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        ConfigureEventSourcing configure)
    {
        var builder = new EventSourcingBuilder();
        configure(builder);
        builder.ConfigureServices(services);
        return services
            .AddTransient(typeof(IAggregateRepository<,>), typeof(AggregateRepository<,>));
    }

    public static EventSourcingBuilder UseAesCryptoProvider(this EventSourcingBuilder builder)
        => builder.UseCryptoProvider<AesCryptoProvider>();
}

public interface IAggregateBuilder<out TBuilder>
    where TBuilder : IAggregateBuilder<TBuilder>
{
    TBuilder UseSnapshotProvider(ISnapshotProvider provider);
}

public sealed class EventSourcingBuilder : IAggregateBuilder<EventSourcingBuilder>
{
    private readonly Dictionary<Type, (ImmutableArray<Type> StateType, AggregateBuilder Builder)> _aggregates = new();
    private readonly AggregateBuilder _defaults = new();
    private Type _cryptoProviderType = typeof(AesCryptoProvider);
    private IEventStoreProvider? _eventStoreProvider;

    public AggregateBuilder AddAggregate<TAggregateId, TState>()
        where TAggregateId : IAggregateId
        where TState : IAggregateState<TState, TAggregateId>
        => CreateAggregateBuilder(typeof(TAggregateId), ImmutableArray.Create(typeof(TState)));

    public AggregateBuilder AddAggregate<TAggregateId>(params Type[] stateTypes)
        where TAggregateId : IAggregateId
    {
        var aggregateIdType = typeof(TAggregateId);
        CheckStateTypes(aggregateIdType, stateTypes);
        return CreateAggregateBuilder(aggregateIdType, stateTypes.ToImmutableArray());
    }

    private AggregateBuilder CreateAggregateBuilder(Type aggregateIdType, ImmutableArray<Type> stateTypes)
    {
        var builder = new AggregateBuilder(GetBuilder(aggregateIdType));
        _aggregates[aggregateIdType] = (stateTypes, builder);
        return builder;
    }

    private AggregateBuilder GetBuilder(Type aggregateIdType)
        => _aggregates.TryGetValue(aggregateIdType, out var configuration)
            ? configuration.Builder
            : _defaults;

    private static void CheckStateTypes(Type aggregateIdType, IEnumerable<Type> stateTypes)
    {
        var exceptions = stateTypes
            .Where(stateType => !IsValidStateType(stateType, aggregateIdType))
            .Select(stateType => new InvalidOperationException(
                $"Type {stateType.Name} is not assignable to type IAggregateState<TState, TAggregateId>"))
            .ToList();

        switch (exceptions)
        {
            // cannot use List Pattern here because it is not supported by Sonar Analyzer
            case {Count: 1}:
                throw exceptions.First();

            case {Count: > 1}:
                throw new AggregateException(exceptions);
        }
    }

    private static bool IsValidStateType(Type stateType, Type aggregateIdType)
    {
        var hasAggregateIdType = false;
        foreach (var typeArguments in GetAggregateStateTypeArguments(stateType))
        {
            if (typeArguments[0] != stateType) return false;
            if (typeArguments[1] != aggregateIdType) continue;

            hasAggregateIdType = true;
        }

        return hasAggregateIdType;
    }

    private static IEnumerable<Type[]> GetAggregateStateTypeArguments(Type stateType)
        => stateType.GetInterfaces()
            .Where(x => x.IsGenericType)
            .Where(x => x.GetGenericTypeDefinition() == typeof(IAggregateState<,>))
            .Select(x => x.GenericTypeArguments);

    public AggregateBuilder Scan(params Type[] assemblyMarkerTypes)
        => Scan(
            assemblyMarkerTypes
                .Select(type => type.Assembly)
                .Distinct()
                .ToArray());

    private AggregateBuilder Scan(params Assembly[] assemblies)
    {
        var builder = new AggregateBuilder(_defaults);

        foreach (var (idType, stateTypes) in assemblies.SelectMany(Scan))
        {
            _aggregates[idType] = (stateTypes, builder);
        }

        return builder;
    }

    private IEnumerable<(Type IdType, ImmutableArray<Type> StateTypes)> Scan(Assembly assembly)
        => assembly.GetTypes()
            .Where(type => !type.IsAbstract)
            .SelectMany(GetAggregateIdAndStateTypes)
            .GroupBy(
                x => x.IdType,
                x => x.StateType,
                (idType, stateTypes) => (idType, stateTypes.ToImmutableArray()));
    
    private static IEnumerable<(Type IdType, Type StateType)> GetAggregateIdAndStateTypes(Type type)
    {
        foreach (var @interface in type.GetInterfaces())
        {
            if (!@interface.IsGenericType) continue;
            var genericTypeDefinition = @interface.GetGenericTypeDefinition();
            if (genericTypeDefinition != typeof(IAggregateState<,>)) continue;
            if (@interface.GenericTypeArguments is not [{ } stateType, { } aggregateIdType]) continue;

            yield return (aggregateIdType, stateType);
        }
    }

    internal void ConfigureServices(
        IServiceCollection services)
    {        
        if (_eventStoreProvider is not { } eventStoreProvider)
            throw new InvalidOperationException($"Event store provider was not specified");

        eventStoreProvider.RegisterServices(services);

        foreach (var (aggregateIdType, (stateTypes, builder)) in _aggregates)
        {
            builder.ConfigureServices(services, aggregateIdType, stateTypes);
        }

        if (_cryptoProviderType is { } cryptoProviderType)
        {
            services.AddTransient(typeof(ICryptoProvider), cryptoProviderType);
        }
    }

    public EventSourcingBuilder UseEventStoreProvider(IEventStoreProvider provider)
    {
        _eventStoreProvider = provider;
        return this;
    }

    public EventSourcingBuilder UseSnapshotProvider(ISnapshotProvider provider)
    {
        _defaults.UseSnapshotProvider(provider);
        return this;
    }

    public EventSourcingBuilder UseCryptoProvider<TCryptoProvider>()
        where TCryptoProvider : ICryptoProvider
    {
        _cryptoProviderType = typeof(TCryptoProvider);
        return this;
    }
}

public interface ISnapshotProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType, Type stateType);

    public sealed void RegisterServices(IServiceCollection services, Type aggregateIdType, IEnumerable<Type> stateTypes)
    {
        foreach (var stateType in stateTypes)
        {
            RegisterServices(services, aggregateIdType, stateType);
        }
    }
}

public interface IEventStoreProvider
{
    public void RegisterServices(IServiceCollection services);
}

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