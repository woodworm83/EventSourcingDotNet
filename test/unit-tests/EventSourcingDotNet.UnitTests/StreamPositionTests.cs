using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.UnitTests;

public class StreamPositionTests
{
    [Fact]
    public void ShouldReturnInitializedPosition()
    {
        var streamPosition = new StreamPosition {Position = 42};

        streamPosition.Position.Should().Be(42);
    }
}