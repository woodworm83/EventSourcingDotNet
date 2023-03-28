using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace EventSourcingDotNet.EventStore.UnitTests;

public static class EventStoreTestContainer
{
    public static IContainer BuildContainer()
        => new ContainerBuilder()
                .WithImage("eventstore/eventstore")
                .WithEnvironment(
                    new Dictionary<string, string>
                    {
                        {"EVENTSTORE_INSECURE", "true"},
                        {"EVENTSTORE_CLUSTER_SIZE", "1"},
                        {"EVENTSTORE_RUN_PROJECTIONS", "All"},
                        {"EVENTSTORE_START_STANDARD_PROJECTIONS", "true"},
                        {"EVENTSTORE_EXT_TCP_PORT", "1113"},
                        {"EVENTSTORE_HTTP_PORT", "2113"},
                        {"EVENTSTORE_ENABLE_EXTERNAL_TCP", "true"},
                        {"EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true"},
                    })
                .WithPortBinding(2113, true)
                .WithAutoRemove(true)
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilPortIsAvailable(2113))
                .WithOutputConsumer(new ConsoleOutputConsumer())
                .Build();

    private sealed class ConsoleOutputConsumer : IOutputConsumer
    {
        public void Dispose()
        {
            Stdout.Dispose();
            Stderr.Dispose();
        }

        public bool Enabled => true;
        public Stream Stdout { get; } = Console.OpenStandardOutput();
        public Stream Stderr { get; } = Console.OpenStandardError();
    }
}