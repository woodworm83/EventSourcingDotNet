using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

[Generator]
public sealed partial class EventSerializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context
                .CompilationProvider
                .SelectMany(GetDomainEvents)
                .Collect(),
            GenerateSource);
    }

    private static IEnumerable<Event> GetDomainEvents(
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var root = syntaxTree.GetRoot(cancellationToken);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            foreach (var classDeclaration in root.DescendantNodes().OfType<RecordDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not { } symbol) continue;
                if (symbol.IsAbstract) continue;

                if (symbol.AllInterfaces.FirstOrDefault(IsDomainEventInterface) is not { } domainEventInterface)
                {
                    continue;
                }

                if (GetAggregateIdType(domainEventInterface) is not { } aggregateIdType)
                {
                    continue;
                }

                yield return new(symbol, aggregateIdType);
            }
        }
    }

    private static NameSyntax GetEventType(INamedTypeSymbol eventType) => ParseName($"global::{eventType}");

    private static NameSyntax? GetAggregateIdType(INamedTypeSymbol domainEventInterface)
        => domainEventInterface switch
        {
            { IsGenericType: true, TypeArguments.Length: 1 }
                => ParseName($"global::{domainEventInterface.TypeArguments[0]}"),
            _ => null,
        };

    private static bool IsDomainEventInterface(INamedTypeSymbol @interface)
        => string.Equals(@interface.Name, "IDomainEvent", StringComparison.Ordinal)
            && string.Equals(
                @interface.ContainingNamespace.ToString(),
                "EventSourcingDotNet",
                StringComparison.Ordinal);

    private static void GenerateSource(
        SourceProductionContext context,
        ImmutableArray<Event> domainEvents)
    {
        try
        {
            context.AddSource(
                "EventSerializer.g",
                SourceText.From(
                    CompilationUnit()
                        .AddMembers(CreateNamespace(domainEvents))
                        .WithLeadingTrivia(
                            Trivia(
                                NullableDirectiveTrivia(
                                    Token(SyntaxKind.EnableKeyword),
                                    isActive: true)))
                        .NormalizeWhitespace()
                        .ToFullString(),
                    Encoding.UTF8));
        }
        catch (Exception exception)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.SourceGenerationFailed,
                    Location.None,
                    exception));
        }
    }

    private static BaseNamespaceDeclarationSyntax CreateNamespace(ImmutableArray<Event> domainEvents)
        => FileScopedNamespaceDeclaration(IdentifierName("EventSourcingDotNet.KurrentDB"))
            .AddMembers(CreateEventSerializerClass(domainEvents))
            .NormalizeWhitespace();

    private static ClassDeclarationSyntax CreateEventSerializerClass(ImmutableArray<Event> domainEvents)
        => ClassDeclaration("EventSerializer")
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword))
            .AddBaseListTypes(SimpleBaseType(Definitions.EventSourcingDotNet.KurrentDB.IEventSerializer.Type))
            .AddParameterListParameters(
                Parameter(Identifier("serializerContext"))
                    .WithType(Definitions.System.Text.Json.Serialization.JsonSerializerContext.Type))
            .AddMembers([..CreateSerializeMembers(domainEvents), ..CreateDeserializeMembers(domainEvents),]);

    private sealed class Event(INamedTypeSymbol symbol, TypeSyntax aggregateIdType)
    {
        public INamedTypeSymbol Symbol { get; } = symbol;

        public TypeSyntax Type { get; } = GetEventType(symbol);

        public TypeSyntax AggregateIdType { get; } = aggregateIdType;

        public string SafeName { get; } = string.Join(
            "_",
            symbol
                .ToDisplayParts(
                    new SymbolDisplayFormat(
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle
                            .NameAndContainingTypesAndNamespaces))
                .Where(part => part.Kind is SymbolDisplayPartKind.ClassName
                    or SymbolDisplayPartKind.RecordClassName
                    or SymbolDisplayPartKind.NamespaceName));
    }
}