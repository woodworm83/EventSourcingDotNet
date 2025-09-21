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
}