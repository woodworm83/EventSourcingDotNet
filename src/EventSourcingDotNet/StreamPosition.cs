namespace EventSourcingDotNet;

public readonly record struct StreamPosition(ulong Position)
{
    public static StreamPosition Start { get; } = new(0);
    public static StreamPosition End { get; } = new(ulong.MaxValue);
}