using System.Reactive.Linq;

namespace EventSourcingDotNet.EventStore;

public static class ObservableExtensions
{
    public static IObservable<T> NotNull<T>(this IObservable<T?> source)
        => source
            .Where(x => x is not null)
            .Select(x => x!);
}