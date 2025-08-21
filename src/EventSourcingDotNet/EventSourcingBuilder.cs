using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet;

public sealed class EventSourcingBuilder : IAggregateBuilder<EventSourcingBuilder>
{
    private readonly Dictionary<Type, (ImmutableArray<Type> StateType, AggregateBuilder Builder)> _aggregates = new();
    private readonly AggregateBuilder _defaults = new();
    private Type _cryptoProviderType = typeof(AesCryptoProvider);
    private IEventStoreProvider? _eventStoreProvider;

    public AggregateBuilder AddAggregate<TAggregateId, TState>()
        where TAggregateId : IAggregateId
        where TState : IAggregateState<TState, TAggregateId>
        => CreateAggregateBuilder(typeof(TAggregateId), [typeof(TState)]);

    public AggregateBuilder AddAggregate<TAggregateId>(params Type[] stateTypes)
        where TAggregateId : IAggregateId
    {
        var aggregateIdType = typeof(TAggregateId);
        CheckStateTypes(stateTypes);
        return CreateAggregateBuilder(aggregateIdType, [..stateTypes]);
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

    private static void CheckStateTypes(IEnumerable<Type> stateTypes)
    {
        var exceptions = stateTypes
            .Where(stateType => !IsValidStateType(stateType))
            .Select(stateType => new InvalidOperationException(
                $"Type {stateType.Name} is not assignable to type IAggregateState<TState, TAggregateId>"))
            .ToList();

        switch (exceptions)
        {
            // cannot use List Pattern here because it is not supported by Sonar Analyzer
            case [var exception]:
                throw exception;
            case {Count: > 0}:
                throw new AggregateException(exceptions);
        }
    }

    private static bool IsValidStateType(Type stateType)
    {
        var hasAggregateIdType = false;
        foreach (var typeArguments in GetAggregateStateTypeArguments(stateType))
        {
            if (typeArguments[0] != stateType) return false;

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

    private static IEnumerable<(Type IdType, ImmutableArray<Type> StateTypes)> Scan(Assembly assembly)
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