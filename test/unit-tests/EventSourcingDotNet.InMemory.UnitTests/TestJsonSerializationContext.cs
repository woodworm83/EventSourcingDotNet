using System.Text.Json.Serialization;

namespace EventSourcingDotNet.InMemory.UnitTests;

[JsonSerializable(typeof(TestEvent))]
[JsonSerializable(typeof(TestId))]
internal sealed partial class TestJsonSerializationContext : JsonSerializerContext;