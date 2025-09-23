namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record TestAggregate : IAggregateState<TestAggregate, TestAggregateId>
{
    public string? Value { get; init; }

    public TestAggregate ApplyEvent(IDomainEvent<TestAggregateId> @event)
        => @event switch
        {
            EncryptedTestEvent { Value: "Value" } encryptedEvent => new() { Value = encryptedEvent.Value },
            _ => this,
        };
}