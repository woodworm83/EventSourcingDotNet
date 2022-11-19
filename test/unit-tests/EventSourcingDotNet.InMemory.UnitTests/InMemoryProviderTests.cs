using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class EventStoreProviderTests
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddEventSourcing(
            builder =>
            {
                builder.AddAggregate<TestId>()
                    .UseInMemoryEventStore();
            })
        .BuildServiceProvider();
    
    [Fact]
    public void EventStoreCanBeResolved()
    {
        _serviceProvider.GetService<IEventStore<TestId>>()
            .Should()
            .BeOfType<InMemoryEventStore<TestId>>();
    }

    [Fact]
    public void EventPublisherCanBeResolved()
    {
        _serviceProvider.GetService<IEventPublisher<TestId>>()
            .Should()
            .BeOfType<InMemoryEventStore<TestId>>();
    }
}