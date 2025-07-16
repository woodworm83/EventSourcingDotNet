using Newtonsoft.Json;

namespace EventSourcingDotNet.KurrentDB;

public sealed record EventSerializerSettings(JsonSerializerSettings? SerializerSettings = null);