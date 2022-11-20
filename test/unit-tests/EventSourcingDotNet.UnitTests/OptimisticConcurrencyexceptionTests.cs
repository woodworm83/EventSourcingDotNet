using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.UnitTests;

public class OptimisticConcurrencyexceptionTests
{
    [Fact]
    public void ShouldReturnExpectedVersion()
    {
        var expectedVersion = new AggregateVersion(42);
        
        var exception = new OptimisticConcurrencyException(expectedVersion, new AggregateVersion());

        exception.ExpectedVersion.Should().Be(expectedVersion);
    }

    [Fact]
    public void ShouldReturnActualVersion()
    {
        var actualVersion = new AggregateVersion(42);

        var exception = new OptimisticConcurrencyException(new AggregateVersion(), actualVersion);

        exception.ActualVersion.Should().Be(actualVersion);
    }
}