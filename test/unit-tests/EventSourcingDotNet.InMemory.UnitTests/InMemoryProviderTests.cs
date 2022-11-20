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
                var aggregateBuilder = builder.AddAggregate<TestId, TestState>()
                    .UseInMemoryEventStore();
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
        
        serviceProvider.GetService<IEventPublisher<TestId>>()
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