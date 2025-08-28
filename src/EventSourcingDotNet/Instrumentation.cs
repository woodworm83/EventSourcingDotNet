using System.Diagnostics;

namespace EventSourcingDotNet;

public static class Instrumentation
{
    public static ActivitySource ActivitySource { get; } = new ActivitySource("EventSourcingDotNet");
}