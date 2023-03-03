using LiteDB;

namespace EventSourcingDotNet.LiteDb;

public sealed record EventRecord(
    [property: BsonId] ObjectId Id,
    string AggregateName,
    string? AggregateId,
    long StreamPosition,
    long AggregateVersion,
    string EventType,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime Timestamp,
    BsonDocument EventData);