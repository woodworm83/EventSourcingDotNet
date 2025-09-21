using System.Text.Json.Serialization;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

[JsonSerializable(typeof(TestEvent))]
public sealed partial class TestJsonSerializerContext : JsonSerializerContext;