using System.Diagnostics;

namespace EventSourcingDotNet;

internal static class Instrumentation
{
    public const string ActivitySourceName = "EventSourcingDotNet";
    
    public static ActivitySource ActivitySource { get; } = new(ActivitySourceName);
}