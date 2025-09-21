using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

using Event = (INamedTypeSymbol Symbol, NameSyntax Type);

[Generator]
public sealed class EventSerializerGenerator : IIncrementalGenerator
{
    private static readonly ThrowExpressionSyntax _throwUnreachableExpression = ThrowExpression(
        ObjectCreationExpression(Definitions.System.Diagnostics.UnreachableException.Type)
            .AddArgumentListArguments());

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider.SelectMany(GetDomainEvents).Collect(),
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
                if (!symbol.AllInterfaces.Any(IsDomainEventInterface)) continue;

                yield return (symbol, GetEventType(symbol));
            }
        }
    }

    private static QualifiedNameSyntax GetEventType(INamedTypeSymbol eventType)
        => QualifiedName(
            GetNamespace(eventType.ContainingNamespace),
            IdentifierName(eventType.Name));

    private static NameSyntax GetNamespace(INamespaceSymbol symbol)
        => symbol.ContainingNamespace is not { Name: "" }
            ? QualifiedName(
                GetNamespace(symbol.ContainingNamespace),
                IdentifierName(symbol.Name))
            : IdentifierName(symbol.Name);

    private static bool IsDomainEventInterface(INamedTypeSymbol @interface)
        => string.Equals(@interface.Name, "IDomainEvent", StringComparison.Ordinal)
            && string.Equals(
                GetNamespace(@interface.ContainingNamespace).ToString(),
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
                        .WithLeadingTrivia(
                            Trivia(
                                NullableDirectiveTrivia(
                                    Token(SyntaxKind.EnableKeyword),
                                    isActive: true)))
                        .AddMembers(CreateNamespace(domainEvents))
                        .NormalizeWhitespace(elasticTrivia: true)
                        .ToFullString(),
                    Encoding.UTF8));
        }
        catch (Exception exception)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.SourceGenerationFailed,
                    location: null,
                    exception));
        }
    }

    private static FileScopedNamespaceDeclarationSyntax CreateNamespace(ImmutableArray<Event> domainEvents)
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
                    .WithType(NullableType(Definitions.System.Text.Json.Serialization.JsonSerializerContext.Type))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))))
            .AddMembers(
                CreateSerializeMethod(),
                CreateDeserializeMethod(),
                CreateEventDataSerializationMethodSelector(domainEvents))
            .AddMembers(
                domainEvents
                    .Select(CreateSerializeEventDataMethod)
                    .ToArray<MemberDeclarationSyntax>());

    private static MethodDeclarationSyntax CreateSerializeMethod()
        => MethodDeclaration(
                GenericName("ValueTask")
                    .AddTypeArgumentListArguments(Definitions.KurrentDB.Client.EventData.Type),
                "SerializeAsync")
            .AddTypeParameterListParameters(TypeParameter("TAggregateId"))
            .AddConstraintClauses(
                TypeParameterConstraintClause(IdentifierName("TAggregateId"))
                    .AddConstraints(TypeConstraint(IdentifierName("IAggregateId"))))
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.AsyncKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("aggregateId"))
                    .WithType(IdentifierName("TAggregateId")),
                Parameter(Identifier("@event"))
                    .WithType(IdentifierName("IDomainEvent")),
                Parameter(Identifier("correlationId"))
                    .WithType(NullableType(IdentifierName("CorrelationId")))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                Parameter(Identifier("causationId"))
                    .WithType(NullableType(IdentifierName("CausationId")))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))))
            .WithExpressionBody(
                ArrowExpressionClause(
                    ObjectCreationExpression(Definitions.KurrentDB.Client.EventData.Type)
                        .AddArgumentListArguments(
                            Argument(
                                InvocationExpression(
                                    QualifiedName(
                                        Definitions.KurrentDB.Client.Uuid.Type,
                                        IdentifierName("NewUuid")))),
                            Argument(
                                MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        InvocationExpression(
                                            QualifiedName(
                                                IdentifierName("@event"),
                                                IdentifierName("GetType"))),
                                        IdentifierName("Name"))
                                    .WithOperatorToken(Token(SyntaxKind.DotToken))),
                            Argument(
                                AwaitExpression(
                                    InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                InvocationExpression(IdentifierName("SerializeEventData"))
                                                    .AddArgumentListArguments(Argument(IdentifierName("@event"))),
                                                IdentifierName("ConfigureAwait")))
                                        .AddArgumentListArguments(
                                            Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression))))))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateDeserializeMethod()
        => MethodDeclaration(
                GenericName("ValueTask")
                    .AddTypeArgumentListArguments(NullableType(Definitions.EventSourcingDotNet.IResolvedEvent.Type)),
                "DeserializeAsync")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("resolvedEvent"))
                    .WithType(Definitions.KurrentDB.Client.ResolvedEvent.Type))
            .AddBodyStatements(
                ReturnStatement(
                    ImplicitObjectCreationExpression()
                        .AddArgumentListArguments(
                            Argument(
                                CastExpression(
                                    NullableType(Definitions.EventSourcingDotNet.IResolvedEvent.Type),
                                    LiteralExpression(SyntaxKind.NullLiteralExpression))))));

    private static MemberDeclarationSyntax CreateEventDataSerializationMethodSelector(
        ImmutableArray<Event> domainEventTypes)
        => MethodDeclaration(
                GenericName("ValueTask")
                    .AddTypeArgumentListArguments(
                        ArrayType(
                            PredefinedType(Token(SyntaxKind.ByteKeyword)),
                            [ArrayRankSpecifier()])),
                "SerializeEventData")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("@event"))
                    .WithType(IdentifierName("IDomainEvent")))
            .WithExpressionBody(
                ArrowExpressionClause(
                    SwitchExpression(IdentifierName("@event"))
                        .AddArms(
                        [
                            ..CreateSerializeEventDataArms(domainEventTypes),
                            SwitchExpressionArm(DiscardPattern(), _throwUnreachableExpression),
                        ])))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static IEnumerable<SwitchExpressionArmSyntax> CreateSerializeEventDataArms(
        ImmutableArray<Event> domainEventTypes)
        => domainEventTypes
            .Select(eventType =>
                SwitchExpressionArm(
                    DeclarationPattern(
                        eventType.Type,
                        SingleVariableDesignation(Identifier("typedEvent"))),
                    InvocationExpression(IdentifierName("SerializeEventData"))
                        .AddArgumentListArguments(Argument(IdentifierName("typedEvent")))));

    private static MethodDeclarationSyntax CreateSerializeEventDataMethod(Event domainEvent)
        => MethodDeclaration(
                GenericName("ValueTask")
                    .AddTypeArgumentListArguments(
                        ArrayType(
                            PredefinedType(Token(SyntaxKind.ByteKeyword)),
                            [ArrayRankSpecifier()])),
                "SerializeEventData")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("@event"))
                    .WithType(domainEvent.Type))
            .WithExpressionBody(
                ArrowExpressionClause(
                    ImplicitObjectCreationExpression()
                        .AddArgumentListArguments(Argument(CreateSerializeExpression(domainEvent)))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static ConditionalExpressionSyntax CreateSerializeExpression(Event domainEvent)
        => ConditionalExpression(
            IsPatternExpression(
                InvocationExpression(
                        ConditionalAccessExpression(
                            IdentifierName("serializerContext"),
                            MemberBindingExpression(
                                Definitions.System.Text.Json.Serialization.JsonSerializerContext
                                    .GetTypeInfoMethod)))
                    .AddArgumentListArguments(Argument(TypeOfExpression(domainEvent.Type))),
                RecursivePattern()
                    .WithPropertyPatternClause(PropertyPatternClause(SeparatedList<SubpatternSyntax>()))
                    .WithDesignation(SingleVariableDesignation(Identifier("typeInfo")))),
            InvocationExpression(Definitions.System.Text.Json.JsonSerializer.SerializeToUtf8BytesMethod)
                .AddArgumentListArguments(
                    Argument(IdentifierName("@event")),
                    Argument(IdentifierName("typeInfo"))),
            InvocationExpression(Definitions.System.Text.Json.JsonSerializer.SerializeToUtf8BytesMethod)
                .AddArgumentListArguments(Argument(IdentifierName("@event"))));
}