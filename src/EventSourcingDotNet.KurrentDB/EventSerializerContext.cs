using System.Text.Json.Serialization;

namespace EventSourcingDotNet.KurrentDB;

[JsonSerializable(typeof(IDomainEvent), TypeInfoPropertyName = "DomainEvent")]
public sealed partial class EventSerializerContext : JsonSerializerContext;