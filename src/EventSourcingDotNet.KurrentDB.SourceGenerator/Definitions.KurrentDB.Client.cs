using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Definitions
{
    public static partial class KurrentDB
    {
        public static class Client
        {
            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            private static NameSyntax Namespace { get; }
                = QualifiedName(
                    Definitions.KurrentDB.Namespace,
                    IdentifierName("Client"));

            public static class ResolvedEvent
            {
                public static TypeSyntax Type { get; }
                    = QualifiedName(
                        Namespace,
                        IdentifierName("ResolvedEvent"));

                public static MemberAccessExpressionSyntax Event(ExpressionSyntax instance)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        instance,
                        IdentifierName("Event"));

                public static MemberAccessExpressionSyntax OriginalEvent(ExpressionSyntax instance)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        instance,
                        IdentifierName("OriginalEvent"));
            }

            public static class EventData
            {
                public static NameSyntax Type { get; }
                    = QualifiedName(
                        Namespace,
                        IdentifierName("EventData"));

                public static ObjectCreationExpressionSyntax New(
                    ExpressionSyntax eventId,
                    ExpressionSyntax eventType,
                    ExpressionSyntax eventData,
                    ExpressionSyntax metadata)
                    => ObjectCreationExpression(Type)
                        .AddArgumentListArguments(
                            Argument(eventId),
                            Argument(eventType),
                            Argument(eventData),
                            Argument(metadata));
            }

            public static class EventRecord
            {
                public static MemberAccessExpressionSyntax EventStreamId(ExpressionSyntax eventRecord)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        eventRecord,
                        IdentifierName("EventStreamId"));

                public static MemberAccessExpressionSyntax EventId(ExpressionSyntax eventRecord)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        eventRecord,
                        IdentifierName("EventId"));

                public static MemberAccessExpressionSyntax EventNumber(ExpressionSyntax eventRecord)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        eventRecord,
                        IdentifierName("EventNumber"));

                public static MemberAccessExpressionSyntax EventType(ExpressionSyntax eventRecord)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        eventRecord,
                        IdentifierName("EventType"));

                public static MemberAccessExpressionSyntax Data(ExpressionSyntax eventRecord)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        eventRecord,
                        IdentifierName("Data"));

                public static MemberAccessExpressionSyntax Metadata(ExpressionSyntax eventRecord)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        eventRecord,
                        IdentifierName("Metadata"));

                public static MemberAccessExpressionSyntax Created(ExpressionSyntax eventRecord)
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        eventRecord,
                        IdentifierName("Created"));
            }

            public static class Uuid
            {
                public static NameSyntax Type { get; }
                    = QualifiedName(
                        Namespace,
                        IdentifierName("Uuid"));

                public static InvocationExpressionSyntax NewUuid
                    => InvocationExpression(QualifiedName(Type, IdentifierName("NewUuid")));

                public static InvocationExpressionSyntax ToGuid(ExpressionSyntax uuid)
                    => InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            uuid,
                            IdentifierName("ToGuid")));
            }

            public static class StreamPosition
            {
                public static InvocationExpressionSyntax ToUInt64(ExpressionSyntax streamPosition)
                    => InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            streamPosition,
                            IdentifierName("ToUInt64")));
            }

            public static class KurrentDBClient
            {
                public static TypeSyntax Type { get; }
                    = QualifiedName(Namespace, IdentifierName("KurrentDBClient"));
            }

            public static class KurrentDBClientSettings
            {
                public static NameSyntax Type { get; }
                    = QualifiedName(
                        Namespace,
                        IdentifierName("KurrentDBClientSettings"));
            }
        }
    }
}