using AwesomeAssertions;
using Xunit;

namespace EventSourcingDotNet.UnitTests;

public sealed class AggregateVersionTests
{
    [Fact]
    public void ShouldReturnInitializedAggregateId()
    {
        var version = new AggregateVersion {Version = 42};

        version.Version.Should().Be(42);
    }
}