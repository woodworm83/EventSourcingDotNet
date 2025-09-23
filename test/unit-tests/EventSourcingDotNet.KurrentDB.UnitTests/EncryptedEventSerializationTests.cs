using AwesomeAssertions;
using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed class EncryptedEventSerializationTests
{
    private readonly EventSerializer _serializer = new(TestJsonSerializerContext.Default);

    [Fact]
    public async Task ShouldDecryptEncryptedProperties()
    {
        var aggregateId = new TestAggregateId(1);
        var @event = new EncryptedTestEvent("secret");
        var streamName = StreamNamingConvention.GetAggregateStreamName(aggregateId);

        var serialized = await _serializer.SerializeAsync(aggregateId, @event, correlationId: null, causationId: null);
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(serialized, streamName);
        var deserialized = await _serializer.DeserializeAsync(resolvedEvent);

        deserialized?.Event.Should().Be(@event);
    }
}