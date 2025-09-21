using FluentAssertions;
using KurrentDB.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed class EventStoreRegistrationTests
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
                .UseKurrentDB(KurrentDBClientSettings.Create("esdb://localhost:2113"))
                .AddAggregate<TestId>())
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .BuildServiceProvider();
}