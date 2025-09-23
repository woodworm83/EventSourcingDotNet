using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

using Event = (INamedTypeSymbol Symbol, TypeSyntax Type, TypeSyntax AggregateIdType);

public sealed partial class EventSerializerGenerator
{
    private static IEnumerable<MemberDeclarationSyntax> CreateSerializeMembers(ImmutableArray<Event> domainEvents)
        =>
        [
            CreateSerializeAsyncMethod(),
            CreateSerializeEventMethod(),
            CreateSerializeEventDataSelectorMethod(domainEvents),
            ..domainEvents.Select(CreateSerializeEventDataMethod),
        ];

    private static MethodDeclarationSyntax CreateSerializeAsyncMethod()
        => MethodDeclaration(
                GenericName("ValueTask")
                    .AddTypeArgumentListArguments(Definitions.KurrentDB.Client.EventData.Type),
                "SerializeAsync")
            .AddTypeParameterListParameters(TypeParameter("TAggregateId"))
            .AddConstraintClauses(
                TypeParameterConstraintClause(IdentifierName("TAggregateId"))
                    .AddConstraints(TypeConstraint(IdentifierName("IAggregateId"))))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("aggregateId"))
                    .WithType(IdentifierName("TAggregateId")),
                Parameter(Identifier("@event"))
                    .WithType(Definitions.EventSourcingDotNet.IDomainEvent.GenericType(IdentifierName("TAggregateId"))),
                Parameter(Identifier("correlationId"))
                    .WithType(NullableType(Definitions.EventSourcingDotNet.CorrelationId.Type))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                Parameter(Identifier("causationId"))
                    .WithType(NullableType(Definitions.EventSourcingDotNet.CausationId.Type))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))))
            .WithExpressionBody(
                ArrowExpressionClause(
                    Definitions.System.Threading.Tasks.ValueTask.GenericNew(
                        Definitions.KurrentDB.Client.EventData.Type,
                        InvocationExpression(IdentifierName("SerializeEvent"))
                            .AddArgumentListArguments(
                                Argument(IdentifierName("aggregateId")),
                                Argument(IdentifierName("@event")),
                                Argument(IdentifierName("correlationId")),
                                Argument(IdentifierName("causationId"))))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateSerializeEventMethod()
        => MethodDeclaration(
                Definitions.KurrentDB.Client.EventData.Type,
                "SerializeEvent")
            .AddTypeParameterListParameters(TypeParameter("TAggregateId"))
            .AddConstraintClauses(
                TypeParameterConstraintClause(IdentifierName("TAggregateId"))
                    .AddConstraints(TypeConstraint(IdentifierName("IAggregateId"))))
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("aggregateId"))
                    .WithType(IdentifierName("TAggregateId")),
                Parameter(Identifier("@event"))
                    .WithType(Definitions.EventSourcingDotNet.IDomainEvent.GenericType(IdentifierName("TAggregateId"))),
                Parameter(Identifier("correlationId"))
                    .WithType(NullableType(Definitions.EventSourcingDotNet.CorrelationId.Type))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                Parameter(Identifier("causationId"))
                    .WithType(NullableType(Definitions.EventSourcingDotNet.CausationId.Type))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))))
            .WithExpressionBody(
                ArrowExpressionClause(
                    Definitions.KurrentDB.Client.EventData.New(
                        Definitions.KurrentDB.Client.Uuid.NewUuid,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            Definitions.System.Object.GetType(IdentifierName("@event")),
                            IdentifierName("Name")),
                        InvocationExpression(IdentifierName("SerializeEventData"))
                            .AddArgumentListArguments(Argument(IdentifierName("@event"))),
                        Definitions.System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                            Definitions.EventSourcingDotNet.KurrentDB.EventMetadata.GenericNew(
                                IdentifierName("TAggregateId"),
                                IdentifierName("aggregateId"),
                                Definitions.EventSourcingDotNet.CorrelationId.ConditionalId(
                                    IdentifierName("correlationId")),
                                Definitions.EventSourcingDotNet.CausationId.ConditionalId(
                                    IdentifierName("causationId"))),
                            TypeOfExpression(
                                Definitions.EventSourcingDotNet.KurrentDB.EventMetadata.GenericType(
                                    IdentifierName("TAggregateId"))),
                            IdentifierName("serializerContext")))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateSerializeEventDataSelectorMethod(
        ImmutableArray<Event> domainEventTypes)
        => MethodDeclaration(
                Definitions.System.ReadOnlyMemory.GenericType(PredefinedType(Token(SyntaxKind.ByteKeyword))),
                "SerializeEventData")
            .AddParameterListParameters(
                Parameter(Identifier("@event"))
                    .WithType(Definitions.EventSourcingDotNet.IDomainEvent.Type))
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .WithExpressionBody(
                ArrowExpressionClause(
                    SwitchExpression(IdentifierName("@event"))
                        .AddArms(
                        [
                            ..CreateSerializeEventDataArms(domainEventTypes),
                            SwitchExpressionArm(
                                DiscardPattern(),
                                Definitions.System.Diagnostics.UnreachableException.Throw),
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
                Definitions.System.ReadOnlyMemory.GenericType(PredefinedType(Token(SyntaxKind.ByteKeyword))),
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
                Definitions.System.Text.Json.Serialization.JsonSerializerContext
                    .GetTypeInfo(
                        IdentifierName("serializerContext"),
                        TypeOfExpression(domainEvent.Type)),
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