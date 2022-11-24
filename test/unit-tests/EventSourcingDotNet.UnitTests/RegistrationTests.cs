﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace EventSourcingDotNet.UnitTests;

public class RegistrationTests
{
    private readonly Mock<IEventStore<TestId>> _eventStoreMock = new();
    private readonly Mock<IEventStoreProvider> _eventStoreProviderMock = new();
    private readonly Mock<ISnapshotStore<TestId, TestState>> _snapshotStoreMock = new();
    private readonly Mock<ISnapshotProvider> _snapshotProviderMock = new();

    public RegistrationTests()
    {
        _eventStoreProviderMock
            .Setup(x => x.RegisterServices(It.IsAny<IServiceCollection>(), typeof(TestId)))
            .Callback<IServiceCollection, Type>((services, _) => services.AddSingleton(_eventStoreMock.Object));
        _snapshotProviderMock
            .Setup(x => x.RegisterServices(It.IsAny<IServiceCollection>(), typeof(TestId), typeof(TestState)))
            .Callback<IServiceCollection, Type, Type>(
                (services, _, _) => services.AddSingleton(_snapshotStoreMock.Object));
    }

    [Fact]
    public void ShouldThrowAggregateExceptionForMultipleInvalidStateTypes()
    {
        var builder = new EventSourcingBuilder();

        builder.Invoking(x => x.AddAggregate<TestId>(typeof(object), typeof(int)))
            .Should().Throw<AggregateException>()
            .And.InnerExceptions.Should().AllBeOfType<InvalidOperationException>();
    }

    [Fact]
    public void ShouldThrowInvalidOperationExceptionForSingleInvalidStateTypes()
    {
        var builder = new EventSourcingBuilder();

        builder.Invoking(x => x.AddAggregate<TestId>(typeof(TestState), typeof(int)))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ShouldThrowInvalidOperationExceptionWhenEventStoreProviderIsNotSet()
    {
        var builder = new AggregateBuilder();
        var serviceCollectionMock = new Mock<IServiceCollection>();

        builder.Invoking(
            x => x.ConfigureServices(serviceCollectionMock.Object, typeof(TestId), Enumerable.Empty<Type>()))
            .Should()
            .Throw<InvalidOperationException>();
        
        
    }
        

    [Theory]
    [MemberData(nameof(GetAddAggregateMethods))]
    public void EventStoreCanBeResolved(Func<EventSourcingBuilder, AggregateBuilder> addAggregateCallback)
    {
        var serviceProvider = new ServiceCollection()
            .AddEventSourcing(builder => addAggregateCallback(builder)
                .UseEventStoreProvider(_eventStoreProviderMock.Object))
            .BuildServiceProvider();

        var eventStore = serviceProvider.GetService<IEventStore<TestId>>();

        eventStore.Should().Be(_eventStoreMock.Object);
    }

    [Theory]
    [MemberData(nameof(GetAddAggregateMethods))]
    public void SnapshotProviderCanBeResolved(Func<EventSourcingBuilder, AggregateBuilder> addAggregateCallback)
    {
        var serviceProvider = new ServiceCollection()
            .AddEventSourcing(builder => addAggregateCallback(builder)
                .UseEventStoreProvider(_eventStoreProviderMock.Object)
                .UseSnapshotProvider(_snapshotProviderMock.Object))
            .BuildServiceProvider();

        var eventStore = serviceProvider.GetService<ISnapshotStore<TestId, TestState>>();

        eventStore.Should().Be(_snapshotStoreMock.Object);
    }

    private static IEnumerable<object[]> GetAddAggregateMethods()
        => new Func<EventSourcingBuilder, AggregateBuilder>[]
            {
                builder => builder.AddAggregate<TestId, TestState>(),
                builder => builder.AddAggregate<TestId>(typeof(TestState)),
                builder => builder.Scan(typeof(RegistrationTests))
            }
            .Select(method => new object[] {method});
}