using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable MemberCanBePrivate.Global

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
[SuppressMessage(
    "Critical Code Smell",
    "S3218:Inner class members should not shadow outer class \"static\" or type members")]
public static partial class Definitions
{
    public static partial class EventSourcingDotNet
    {
        public static NameSyntax Namespace { get; }
            = IdentifierName("global::EventSourcingDotNet");

        // ReSharper disable once InconsistentNaming
        public static class IDomainEvent
        {
            public static TypeSyntax Type { get; }
                = QualifiedName(Namespace, IdentifierName("IDomainEvent"));

            public static TypeSyntax GenericType(TypeSyntax aggregateIdType)
                => QualifiedName(
                    Namespace,
                    GenericName("IDomainEvent")
                        .AddTypeArgumentListArguments(aggregateIdType));
        }

        // ReSharper disable once InconsistentNaming
        public static class IResolvedEvent
        {
            public static NameSyntax Type { get; }
                = QualifiedName(
                    Namespace,
                    IdentifierName("IResolvedEvent"));
        }

        public static class ResolvedEvent
        {
            public static NameSyntax GenericType(TypeSyntax aggregateIdType)
                => QualifiedName(
                    Namespace,
                    GenericName("ResolvedEvent")
                        .AddTypeArgumentListArguments(aggregateIdType));

            public static ObjectCreationExpressionSyntax GenericNew(
                TypeSyntax aggregateIdType,
                ExpressionSyntax eventId,
                ExpressionSyntax streamName,
                ExpressionSyntax aggregateId,
                ExpressionSyntax aggregateVersion,
                ExpressionSyntax streamPosition,
                ExpressionSyntax @event,
                ExpressionSyntax timestamp,
                ExpressionSyntax correlationId,
                ExpressionSyntax causationId)
                => ObjectCreationExpression(GenericType(aggregateIdType))
                    .AddArgumentListArguments(
                        Argument(eventId),
                        Argument(streamName),
                        Argument(aggregateId),
                        Argument(aggregateVersion),
                        Argument(streamPosition),
                        Argument(@event),
                        Argument(timestamp),
                        Argument(correlationId),
                        Argument(causationId));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static class IEventStoreProvider
        {
            public static NameSyntax Type { get; }
                = QualifiedName(
                    Namespace,
                    IdentifierName("IEventStoreProvider"));
        }

        public static class EventSourcingBuilder
        {
            public static NameSyntax Type { get; }
                = QualifiedName(
                    Namespace,
                    IdentifierName("EventSourcingBuilder"));

            public static InvocationExpressionSyntax UseEventStoreProvider(
                ExpressionSyntax builderInstance,
                ExpressionSyntax provider)
                => InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            builderInstance,
                            IdentifierName("UseEventStoreProvider")))
                    .AddArgumentListArguments(Argument(provider));
        }

        public static class CorrelationId
        {
            public static NameSyntax Type { get; }
                = QualifiedName(Namespace, IdentifierName("CorrelationId"));

            public static ObjectCreationExpressionSyntax New(ExpressionSyntax guid)
                => ObjectCreationExpression(Type)
                    .AddArgumentListArguments(Argument(guid));

            public static ExpressionSyntax NullableNew(ExpressionSyntax guid)
                => ConditionalExpression(
                    IsPatternExpression(
                        guid,
                        UnaryPattern(
                            Token(SyntaxKind.NotKeyword),
                            ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression)))),
                    New(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            guid,
                            IdentifierName("Value"))),
                    LiteralExpression(SyntaxKind.NullLiteralExpression));
            
            public static ExpressionSyntax ConditionalId(ExpressionSyntax correlationId)
                => ConditionalAccessExpression(
                    correlationId,
                    MemberBindingExpression(IdentifierName("Id")));
        }

        public static class CausationId
        {
            public static NameSyntax Type { get; }
                = QualifiedName(Namespace, IdentifierName("CausationId"));

            public static ObjectCreationExpressionSyntax New(ExpressionSyntax guid)
                => ObjectCreationExpression(Type)
                    .AddArgumentListArguments(Argument(guid));

            public static ExpressionSyntax NullableNew(ExpressionSyntax guid)
                => ConditionalExpression(
                    IsPatternExpression(
                        guid,
                        UnaryPattern(
                            Token(SyntaxKind.NotKeyword),
                            ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression)))),
                    New(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            guid,
                            IdentifierName("Value"))),
                    LiteralExpression(SyntaxKind.NullLiteralExpression));

            public static ExpressionSyntax ConditionalId(ExpressionSyntax causationId)
                => ConditionalAccessExpression(
                    causationId,
                    MemberBindingExpression(IdentifierName("Id")));
        }

        public static class EventId
        {
            public static NameSyntax Type { get; }
                = QualifiedName(Namespace, IdentifierName("EventId"));

            public static ObjectCreationExpressionSyntax New(ExpressionSyntax eventId)
                => ObjectCreationExpression(Type)
                    .AddArgumentListArguments(Argument(eventId));
        }

        public static class AggregateVersion
        {
            public static NameSyntax Type { get; }
                = QualifiedName(Namespace, IdentifierName("AggregateVersion"));

            public static ObjectCreationExpressionSyntax New(ExpressionSyntax aggregateVersion)
                => ObjectCreationExpression(Type)
                    .AddArgumentListArguments(Argument(aggregateVersion));
        }

        public static class StreamPosition
        {
            public static NameSyntax Type { get; }
                = QualifiedName(Namespace, IdentifierName("StreamPosition"));

            public static ObjectCreationExpressionSyntax New(ExpressionSyntax streamPosition)
                => ObjectCreationExpression(Type)
                    .AddArgumentListArguments(Argument(streamPosition));
        }
    }
}