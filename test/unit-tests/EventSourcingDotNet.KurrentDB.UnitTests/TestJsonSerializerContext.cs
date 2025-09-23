using System.Text.Json.Serialization;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

[JsonSerializable(typeof(TestEvent))]
[JsonSerializable(typeof(TestAggregateId))]
[JsonSerializable(typeof(EventMetadata<TestAggregateId>))]
[JsonSerializable(typeof(TestAggregateId))]
public sealed partial class TestJsonSerializerContext : JsonSerializerContext;