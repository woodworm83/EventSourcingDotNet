using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class EventStoreRegistrationTests
{
    [Fact]
    public void ShouldResolveEventStore()
    {
        var serviceProvider = BuildServiceProvider();

        var eventStore = serviceProvider.GetService<IEventStore<TestId>>();

        eventStore.Should().BeOfType<EventStore<TestId>>();
    }

    [Fact]
    public void ShouldResolveEventPublisher()
    {
        var serviceProvider = BuildServiceProvider();

        var eventStore = serviceProvider.GetService<IEventListener>();

        eventStore.Should().BeOfType<EventListener>();
    }

    [Theory]
    [InlineData(typeof(IEventListener), typeof(EventListener))]
    [InlineData(typeof(IEventReader), typeof(EventReader))]
    public void ShouldResolveService(Type serviceType, Type implementationType)
    {
        var serviceProvider = BuildServiceProvider();

        var service = serviceProvider.GetService(serviceType);

        service.Should().BeOfType(implementationType);
    }

    private static IServiceProvider BuildServiceProvider()
        => new ServiceCollection()
            .AddEventSourcing(builder => builder
                .UseEventStore("esdb://localhost:2113")
                .AddAggregate<TestId>())
            .AddSingleton<IEventTypeResolver>(new TestEventTypeResolver())
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .BuildServiceProvider();
}