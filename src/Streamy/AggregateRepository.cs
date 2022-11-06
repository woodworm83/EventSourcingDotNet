namespace Streamy;

public interface IAggregateRepository<TAggregateId, TState>
    where TState : IAggregateState<TState, TAggregateId>
{
    Task<Aggregate<TAggregateId, TState>> GetById(TAggregateId id);
}

internal sealed class AggregateRepository<TAggregateId, TState> : IAggregateRepository<TAggregateId, TState>
    where TState : IAggregateState<TState, TAggregateId>
{
    public Task<Aggregate<TAggregateId, TState>> GetById(TAggregateId id)
        => Task.FromResult(new Aggregate<TAggregateId, TState>(id));
}