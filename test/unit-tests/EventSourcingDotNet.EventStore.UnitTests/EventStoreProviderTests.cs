using EventSourcingDotNet.Providers.EventStore;
using EventStore.Client;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class EventStoreProviderTests
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddEventSourcing(
            builder =>
            {
                builder.AddAggregate<TestId>()
                    .UseEventStore();
            })
        .AddSingleton(Options.Create(new EventStoreClientSettings()))
        .BuildServiceProvider();
    
    [Fact]
    public void EventStoreCanBeResolved()
    {
        _serviceProvider.GetService<IEventStore<TestId>>()
            .Should()
            .BeOfType<EventStore<TestId>>();
    }
}