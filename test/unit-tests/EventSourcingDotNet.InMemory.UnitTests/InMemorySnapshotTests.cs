using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class InMemorySnapshotTests
{
    [Fact]
    public async Task ShouldReturnNullWhenNoSnapshotWasStored()
    {
        var snapshot = new InMemorySnapshotStore<TestId, TestState>();

        var result = await snapshot.GetAsync(new TestId());
        
        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldReturnSnapshot()
    {
        var aggregate = new Aggregate<TestId, TestState>(new TestId());
        var snapshot = new InMemorySnapshotStore<TestId, TestState>();
        
        await snapshot.SetAsync(aggregate);

        var result = await snapshot.GetAsync(aggregate.Id);
                
        result.Should().BeSameAs(aggregate);
    }
}