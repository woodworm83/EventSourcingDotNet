using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet;

public readonly record struct CausationId(Guid Id)
{
    public CausationId()
        : this(Guid.NewGuid())
    { }
    
    public static implicit operator CausationId(EventId eventId)
        => new(eventId.Id);

    public static implicit operator EventId(CausationId causationId)
        => new(causationId.Id);
    
    [return: NotNullIfNotNull(nameof(id))]
    internal static CausationId? FromGuid(Guid? id)
        => id.HasValue
            ? new CausationId(id.Value)
            : null;
}