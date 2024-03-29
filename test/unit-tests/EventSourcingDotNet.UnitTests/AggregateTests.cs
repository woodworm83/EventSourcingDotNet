﻿using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.UnitTests;

public class AggregateTests
{
    [Fact]
    public void ShouldHaveEmptyUncommittedEvents()
    {
        var aggregate = new Aggregate<TestId, TestState>(new TestId());

        aggregate.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void ShouldAppendUncommittedEvent()
    {
        var @event = new TestEvent();
        var aggregate = new Aggregate<TestId, TestState>(new TestId());

        var result = aggregate.AddEvent(@event);
        
        result.UncommittedEvents.Should().Equal(@event);
    }

    [Fact]
    public void ShouldUpdateStateWhenEventToFireIsAdded()
    {
        var @event = new ValueUpdatedEvent(42);
        var aggregate = new Aggregate<TestId, TestState>(new TestId());

        var result = aggregate.AddEvent(@event);

        result.State.Value.Should().Be(@event.NewValue);
    }

    [Fact]
    public void ShouldNotUpdateStateWhenEventToSkipIsAdded()
    {
        var @event = new ValueUpdatedEvent(42) {ValidationResult = EventValidationResult.Skip};
        var aggregate = new Aggregate<TestId, TestState>(new TestId());

        var result = aggregate.AddEvent(@event);

        result.State.Value.Should().NotBe(@event.NewValue);
    }

    [Fact]
    public void ShouldThrowExceptionWhenEventValidationFails()
    {
        const string message = "message";
        var @event = new ValueUpdatedEvent(42) {ValidationResult = EventValidationResult.Fail(message)};
        var aggregate = new Aggregate<TestId, TestState>(new TestId());

        aggregate.Invoking(x => x.AddEvent(@event))
            .Should()
            .ThrowExactly<EventValidationException>();
    }

    [Fact]
    public void ShouldThrowInvalidOperationExceptionOnUnsupportedValidationResult()
    {
        var @event = new ValueUpdatedEvent(42) {ValidationResult = null!};
        var aggregate = new Aggregate<TestId, TestState>(new TestId());

        aggregate.Invoking(x => x.AddEvent(@event))
            .Should()
            .Throw<NotSupportedException>();
    }
}