using EventStore.Client;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class EventStoreRegistrationTests
{
    private static ServiceProvider BuildServiceProvider(string? connectionStringForTestAggregate = null)
    {
        return new ServiceCollection()
            .ConfigureEventStore("esdb://localhost:2113")
            .AddEventSourcing(builder => AddTestAggregate(connectionStringForTestAggregate, builder))
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .BuildServiceProvider();
    }

    private static void AddTestAggregate(string? connectionStringForTestAggregate, EventSourcingBuilder builder)
    {
        builder.AddAggregate<TestId>();
        
        if (connectionStringForTestAggregate is not null)
        {
            builder.UseEventStore(connectionStringForTestAggregate);
        }
        else
        {
            builder.UseEventStore();
        }
    }

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

    [Fact]
    public void ShouldResolveEventStoreWithConnectionString()
    {
        const string connectionString = "esdb://localhost:9876";
        var clientSettings = EventStoreClientSettings.Create(connectionString);
        var serviceProvider = BuildServiceProvider(connectionString);

        var eventStore = serviceProvider.GetService<IEventStore<TestId>>();

        eventStore.Should().BeOfType<EventStore<TestId>>()
            .Which
            .ClientSettings.ConnectivitySettings.Should().BeEquivalentTo(clientSettings.ConnectivitySettings);
    }

    [Fact]
    public void ShouldResolveEventPublisherWithConnectionString()
    {
        const string connectionString = "esdb://localhost:9876";
        var clientSettings = EventStoreClientSettings.Create(connectionString);
        var serviceProvider = BuildServiceProvider(connectionString);

        var eventStore = serviceProvider.GetService<IEventListener>();

        eventStore.Should().BeOfType<EventListener>()
            .Which
            .ClientSettings.ConnectivitySettings.Should().BeEquivalentTo(clientSettings.ConnectivitySettings);
    }
}