using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public sealed partial class EventSerializerGenerator
{
    private static IEnumerable<MemberDeclarationSyntax> CreateDeserializeMembers(ImmutableArray<Event> domainEvents)
        =>
        [
            CreateDeserializeAsyncMethod(),
            CreateDeserializeMethod(domainEvents),
            CreateDeserializeEventMetadataMethod(),
            ..domainEvents.Select(CreateDeserializeEventMethod),
        ];

    private static MethodDeclarationSyntax CreateDeserializeAsyncMethod()
        => MethodDeclaration(
                Definitions.System.Threading.Tasks.ValueTask.GenericType(
                    NullableType(Definitions.EventSourcingDotNet.IResolvedEvent.Type)),
                "DeserializeAsync")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("resolvedEvent"))
                    .WithType(Definitions.KurrentDB.Client.ResolvedEvent.Type))
            .WithExpressionBody(
                ArrowExpressionClause(
                    Definitions.System.Threading.Tasks.ValueTask.GenericNew(
                        NullableType(Definitions.EventSourcingDotNet.IResolvedEvent.Type),
                        InvocationExpression(IdentifierName("DeserializeEvent"))
                            .AddArgumentListArguments(Argument(IdentifierName("resolvedEvent"))))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateDeserializeMethod(ImmutableArray<Event> domainEvents)
        => MethodDeclaration(
                NullableType(Definitions.EventSourcingDotNet.IResolvedEvent.Type),
                "DeserializeEvent")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("resolvedEvent"))
                    .WithType(Definitions.KurrentDB.Client.ResolvedEvent.Type))
            .WithExpressionBody(
                ArrowExpressionClause(
                    SwitchExpression(
                        IdentifierName("resolvedEvent"),
                        [
                            ..CreateDeserializeEventArms(domainEvents),
                            SwitchExpressionArm(
                                DiscardPattern(),
                                LiteralExpression(SyntaxKind.NullLiteralExpression)),
                        ])))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static IEnumerable<SwitchExpressionArmSyntax> CreateDeserializeEventArms(ImmutableArray<Event> domainEvents)
        => domainEvents
            .Select(CreateDeserializeEventArm);

    private static SwitchExpressionArmSyntax CreateDeserializeEventArm(Event domainEvent)
        => SwitchExpressionArm(
            CreateDeserializeEventPattern(domainEvent),
            InvocationExpression(IdentifierName($"DeserializeEvent_{domainEvent.SafeName}"))
                .AddArgumentListArguments(
                    Argument(IdentifierName("resolvedEvent"))));

    private static RecursivePatternSyntax CreateDeserializeEventPattern(Event domainEvent)
        => RecursivePattern()
            .AddPropertyPatternClauseSubpatterns(
                Subpattern(
                    NameColon(IdentifierName("Event.EventType")),
                    ConstantPattern(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(domainEvent.Symbol.Name)))));

    private static MethodDeclarationSyntax CreateDeserializeEventMethod(Event domainEvent)
    {
        var resolvedEvent = new Instances.KurrentDB.Client.ResolvedEvent(IdentifierName("resolvedEvent"));
        var eventMetadata = new Instances.EventSourcingDotNet.KurrentDB.EventMetadata(IdentifierName("metadata"));

        return MethodDeclaration(
                NullableType(Definitions.EventSourcingDotNet.ResolvedEvent.GenericType(domainEvent.AggregateIdType)),
                $"DeserializeEvent_{domainEvent.SafeName}")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("resolvedEvent"))
                    .WithType(Definitions.KurrentDB.Client.ResolvedEvent.Type))
            .AddBodyStatements(
                IfStatement(
                    IsPatternExpression(
                        InvocationExpression(
                                GenericName("DeserializeEventMetadata")
                                    .AddTypeArgumentListArguments(domainEvent.AggregateIdType))
                            .AddArgumentListArguments(Argument(IdentifierName("resolvedEvent"))),
                        UnaryPattern(
                            Token(SyntaxKind.NotKeyword),
                            DeclarationPattern(
                                Definitions.EventSourcingDotNet.KurrentDB.EventMetadata.GenericType(
                                    domainEvent.AggregateIdType),
                                SingleVariableDesignation(Identifier("metadata"))))),
                    ReturnStatement(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                ReturnStatement(
                    Definitions
                        .EventSourcingDotNet
                        .ResolvedEvent
                        .GenericNew(
                            domainEvent.AggregateIdType,
                            Definitions.EventSourcingDotNet.EventId.New(resolvedEvent.Event.EventId.ToGuid),
                            resolvedEvent.Event.EventStreamId,
                            eventMetadata.AggregateId,
                            resolvedEvent.Event.EventNumber.ToAggregateVersion(),
                            resolvedEvent.OriginalEvent.EventNumber.ToStreamPosition(),
                            CreateDeserializeEventDataExpression(domainEvent, resolvedEvent),
                            resolvedEvent.Event.Created,
                            Definitions.EventSourcingDotNet.CorrelationId.NullableNew(eventMetadata.CorrelationId),
                            Definitions.EventSourcingDotNet.CausationId.NullableNew(eventMetadata.CausationId))));
    }

    private static ExpressionSyntax CreateDeserializeEventDataExpression(
        Event domainEvent,
        Instances.KurrentDB.Client.ResolvedEvent resolvedEvent)
        => ConditionalExpression(
            IsPatternExpression(
                Definitions.System.Text.Json.Serialization.JsonSerializerContext.GetTypeInfo(
                    IdentifierName("serializerContext"),
                    TypeOfExpression(domainEvent.Type)),
                DeclarationPattern(
                    Definitions.System.Text.Json.Serialization.Metadata.JsonTypeInfo.GenericType(domainEvent.Type),
                    SingleVariableDesignation(Identifier("typeInfo")))),
            Definitions.System.Text.Json.JsonSerializer.Deserialize(
                domainEvent.Type,
                resolvedEvent.Event.DataAsSpan,
                IdentifierName("typeInfo")),
            Definitions.System.Text.Json.JsonSerializer.Deserialize(
                domainEvent.Type,
                resolvedEvent.Event.DataAsSpan));

    private static MethodDeclarationSyntax CreateDeserializeEventMetadataMethod()
    {
        var resolvedEvent = new Instances.KurrentDB.Client.ResolvedEvent(IdentifierName("resolvedEvent"));

        return MethodDeclaration(
                NullableType(
                    Definitions.EventSourcingDotNet.KurrentDB.EventMetadata.GenericType(
                        IdentifierName("TAggregateId"))),
                "DeserializeEventMetadata")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddTypeParameterListParameters(TypeParameter(Identifier("TAggregateId")))
            .AddConstraintClauses(
                TypeParameterConstraintClause(IdentifierName("TAggregateId"))
                    .AddConstraints(TypeConstraint(IdentifierName("IAggregateId"))))
            .AddParameterListParameters(
                Parameter(Identifier("resolvedEvent"))
                    .WithType(Definitions.KurrentDB.Client.ResolvedEvent.Type))
            .AddBodyStatements(
                TryStatement()
                    .AddBlockStatements(
                        ReturnStatement(
                            ConditionalExpression(
                                IsPatternExpression(
                                    Definitions.System.Text.Json.Serialization.JsonSerializerContext.GetTypeInfo(
                                        IdentifierName("serializerContext"),
                                        TypeOfExpression(
                                            Definitions.EventSourcingDotNet.KurrentDB.EventMetadata.GenericType(
                                                IdentifierName("TAggregateId")))),
                                    RecursivePattern()
                                        .WithType(
                                            Definitions.System.Text.Json.Serialization.Metadata.JsonTypeInfo
                                                .GenericType(
                                                    Definitions.EventSourcingDotNet.KurrentDB.EventMetadata.GenericType(
                                                        IdentifierName("TAggregateId"))))
                                        .AddPropertyPatternClauseSubpatterns()
                                        .WithDesignation(SingleVariableDesignation(Identifier("typeInfo")))),
                                Definitions.System.Text.Json.JsonSerializer.Deserialize(
                                    Definitions.EventSourcingDotNet.KurrentDB.EventMetadata.GenericType(
                                        IdentifierName("TAggregateId")),
                                    resolvedEvent.Event.MetadataAsSpan,
                                    IdentifierName("typeInfo")),
                                Definitions.System.Text.Json.JsonSerializer.Deserialize(
                                    Definitions.EventSourcingDotNet.KurrentDB.EventMetadata.GenericType(
                                        IdentifierName("TAggregateId")),
                                    resolvedEvent.Event.MetadataAsSpan))))
                    .AddCatches(
                        CatchClause()
                            .WithDeclaration(CatchDeclaration(Definitions.System.Text.Json.JsonException.Type))
                            .AddBlockStatements(ReturnStatement(LiteralExpression(SyntaxKind.NullLiteralExpression)))));
    }
}