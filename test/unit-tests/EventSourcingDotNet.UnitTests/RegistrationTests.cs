using FluentAssertions;
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
            .Setup(x => x.RegisterServices(It.IsAny<IServiceCollection>()))
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
            .AddEventSourcing(builder => addAggregateCallback(
                builder.UseEventStoreProvider(_eventStoreProviderMock.Object)))
            .BuildServiceProvider();

        var eventStore = serviceProvider.GetService<IEventStore<TestId>>();

        eventStore.Should().Be(_eventStoreMock.Object);
    }

    [Theory]
    [MemberData(nameof(GetAddAggregateMethods))]
    public void SnapshotProviderCanBeResolved(Func<EventSourcingBuilder, AggregateBuilder> addAggregateCallback)
    {
        var serviceProvider = new ServiceCollection()
            .AddEventSourcing(builder => addAggregateCallback(
                    builder.UseEventStoreProvider(_eventStoreProviderMock.Object))
                .UseSnapshotProvider(_snapshotProviderMock.Object))
            .BuildServiceProvider();

        var eventStore = serviceProvider.GetService<ISnapshotStore<TestId, TestState>>();

        eventStore.Should().Be(_snapshotStoreMock.Object);
    }

    [Fact]
    public void ShouldResolveAesCryptoProviderByDefault()
    {
        var serviceProvider = new ServiceCollection()
            .AddEventSourcing(_ => { })
            .BuildServiceProvider();

        serviceProvider.GetService<ICryptoProvider>()
            .Should().BeOfType<AesCryptoProvider>();
    }

    [Fact]
    public void ShouldResolveAesCryptoProviderWhenSpecified()
    {
        var serviceProvider = new ServiceCollection()
            .AddEventSourcing(builder => { builder.UseAesCryptoProvider(); })
            .BuildServiceProvider();

        serviceProvider.GetService<ICryptoProvider>()
            .Should().BeOfType<AesCryptoProvider>();
    }

    [Fact]
    public void ShouldResolveSpecifiedCryptoProvider()
    {
        var serviceProvider = new ServiceCollection()
            .AddEventSourcing(builder => { builder.UseCryptoProvider<TestCryptoProvider>(); })
            .BuildServiceProvider();

        serviceProvider.GetService<ICryptoProvider>()
            .Should().BeOfType<TestCryptoProvider>();
    }

    private static IEnumerable<object[]> GetAddAggregateMethods()
        => new Func<EventSourcingBuilder, AggregateBuilder>[]
            {
                builder => builder.AddAggregate<TestId, TestState>(),
                builder => builder.AddAggregate<TestId>(typeof(TestState)),
                builder => builder.Scan(typeof(RegistrationTests))
            }
            .Select(method => new object[] {method});

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class TestCryptoProvider : ICryptoProvider
    {
        public void Encrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey)
        {
            inputStream.CopyTo(outputStream);
        }

        public bool TryDecrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey)
        {
            inputStream.CopyTo(outputStream);
            return true;
        }

        public EncryptionKey GenerateKey() => new();
    }
}