using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class EventStoreRegistrationTests
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .ConfigureEventStore(new Uri("esdb://localhost"))
        .AddEventSourcing(builder => builder.AddAggregate<TestId>().UseEventStore())
        .BuildServiceProvider();
    
    [Fact]
    public void ShouldResolveEventStore()
    {
        var eventStore = _serviceProvider.GetService<IEventStore<TestId>>();

        eventStore.Should().BeOfType<EventStore<TestId>>();
    }

    [Fact]
    public void ShouldResolveEventPublisher()
    {
        var eventStore = _serviceProvider.GetService<IEventStore<TestId>>();
        
        eventStore.Should().BeOfType<EventStore<TestId>>();
    }
}