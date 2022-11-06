using FluentAssertions;
using Xunit;

namespace Streamy.UnitTests;

public class UnitTest1
{
    [Fact]
    public async Task ShouldCreateNewAggregate()
    {
        var repository = new AggregateRepository<TestId, TestState>();
        
        var result = await repository.GetById(new TestId(42));

        result.Id.Id.Should().Be(42);
    }
}