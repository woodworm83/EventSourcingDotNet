namespace EventSourcingDotNet;

public sealed class EventValidationException(string message) : Exception($"Event validation failed:\n{message}");