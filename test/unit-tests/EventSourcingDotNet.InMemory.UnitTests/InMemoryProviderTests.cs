using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class InMemoryRegistrationTests
{
    private static ServiceProvider CreateServiceProvider(Action<AggregateBuilder>? configureAggregate = null)
    {
        return new ServiceCollection()
            .AddEventSourcing(builder =>
            {
                builder.UseInMemoryEventStore();
                var aggregateBuilder = builder.AddAggregate<TestId, TestState>();
                configureAggregate?.Invoke(aggregateBuilder);
            })
            .BuildServiceProvider();
    }

    [Fact]
    public void EventStoreCanBeResolved()
    {
        var serviceProvider = CreateServiceProvider();
        
        serviceProvider.GetService<IEventStore<TestId>>()
            .Should()
            .BeOfType<InMemoryEventStore<TestId>>();
    }

    [Fact]
    public void EventPublisherCanBeResolved()
    {
        var serviceProvider = CreateServiceProvider();
        
        serviceProvider.GetService<IEventListener>()
            .Should()
            .BeOfType<InMemoryEventStore<TestId>>();
    }

    [Fact]
    public void SnapshotCanBeResolved()
    {
        var serviceProvider = CreateServiceProvider(builder => builder.UseInMemorySnapshot());

        serviceProvider.GetService<ISnapshotStore<TestId, TestState>>()
            .Should()
            .BeOfType<InMemorySnapshot<TestId, TestState>>();
    }
}