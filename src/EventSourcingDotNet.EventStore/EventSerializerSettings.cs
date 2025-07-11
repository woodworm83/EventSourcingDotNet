using Newtonsoft.Json;

namespace EventSourcingDotNet.EventStore;

public sealed record EventSerializerSettings(JsonSerializerSettings? SerializerSettings = null);