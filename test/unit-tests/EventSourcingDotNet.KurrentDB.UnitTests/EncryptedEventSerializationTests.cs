using EventSourcingDotNet.Serialization.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

public class EncryptedEventSerializationTests
{
    private readonly EventSerializer _serializer = new(
        new TestEventTypeResolver(),
        CreateSerializerSettingsFactory());

    [Fact]
    public async Task ShouldDecryptEncryptedProperties()
    {
        var aggregateId = new TestId();
        var @event = new EncryptedTestEvent("secret");
        var streamName = StreamNamingConvention.GetAggregateStreamName(aggregateId);

        var serialized = await _serializer.SerializeAsync(aggregateId, @event);
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(serialized, streamName);
        var deserialized = await _serializer.DeserializeAsync(resolvedEvent);

        deserialized.Event.Should().Be(@event);
    }

    private static JsonSerializerSettingsFactory CreateSerializerSettingsFactory()
    {
        var cryptoProvider = new AesCryptoProvider();
        return new JsonSerializerSettingsFactory(
            NullLoggerFactory.Instance,
            cryptoProvider,
            new TestEncryptionKeyStore(cryptoProvider));
    }
}