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
            .Callback<IServiceCollection>(services => services.AddSingleton(_eventStoreMock.Object));
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
        var builder = new EventSourcingBuilder();
        var serviceCollectionMock = new Mock<IServiceCollection>();

        builder.Invoking(
                x => x.ConfigureServices(serviceCollectionMock.Object))
            .Should()
            .Throw<InvalidOperationException>();
    }


    [Fact]
    public void ShouldResolveEventStore()
    {
        var serviceProvider = BuildServiceProvider();

        var eventStore = serviceProvider.GetService<IEventStore<TestId>>();

        eventStore.Should().Be(_eventStoreMock.Object);
    }

    [Theory]
    [MemberData(nameof(GetAddAggregateMethods))]
    public void SnapshotProviderCanBeResolved(Func<EventSourcingBuilder, AggregateBuilder> addAggregateCallback)
    {
        var serviceProvider = BuildServiceProvider(addAggregateCallback, useSnapshotProvider: true);

        var eventStore = serviceProvider.GetService<ISnapshotStore<TestId, TestState>>();

        eventStore.Should().Be(_snapshotStoreMock.Object);
    }

    [Fact]
    public void ShouldResolveAesCryptoProviderByDefault()
    {
        var serviceProvider = BuildServiceProvider();

        serviceProvider.GetService<ICryptoProvider>()
            .Should().BeOfType<AesCryptoProvider>();
    }

    [Fact]
    public void ShouldResolveAesCryptoProviderWhenSpecified()
    {
        var serviceProvider = BuildServiceProvider(useAesCryptoProvider: true);

        serviceProvider.GetService<ICryptoProvider>()
            .Should().BeOfType<AesCryptoProvider>();
    }

    [Fact]
    public void ShouldResolveSpecifiedCryptoProvider()
    {
        var serviceProvider = BuildServiceProvider(useTestCryptoProvider: true);

        serviceProvider.GetService<ICryptoProvider>()
            .Should().BeOfType<TestCryptoProvider>();
    }

    private IServiceProvider BuildServiceProvider(
        Func<EventSourcingBuilder, AggregateBuilder>? addAggregateCallback = null,
        bool useSnapshotProvider = false,
        bool useTestCryptoProvider = false,
        bool useAesCryptoProvider = false)
        => new ServiceCollection()
            .AddEventSourcing(
                builder =>
                {
                    builder.UseEventStoreProvider(_eventStoreProviderMock.Object);
                    var aggregate = addAggregateCallback?.Invoke(builder);

                    if (useSnapshotProvider)
                        aggregate?.UseSnapshotProvider(_snapshotProviderMock.Object);

                    if (useTestCryptoProvider)
                        builder.UseCryptoProvider<TestCryptoProvider>();

                    if (useAesCryptoProvider)
                        builder.UseAesCryptoProvider();
                })
            .BuildServiceProvider();

    public static IEnumerable<object[]> GetAddAggregateMethods()
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