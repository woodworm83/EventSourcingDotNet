using Microsoft.CodeAnalysis;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor SourceGenerationFailed
        = new(
            DiagnosticIds.SourceGenerationFailed,
            "Failed to generate source",
            "Failed to generate code: {0}",
            "SourceGeneration",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor DomainEventShouldImplementGenericInterface
        = new(
            DiagnosticIds.DomainEventShouldImplementGenericInterface,
            "Domain event skipped",
            "Domain event {0} should implement generic IDomainEvent<TAggregateId> interface",
            "SourceGeneration",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
}