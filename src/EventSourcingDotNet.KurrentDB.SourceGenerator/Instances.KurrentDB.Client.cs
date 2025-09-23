using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Instances
{
    public static class KurrentDB
    {
        public static class Client
        {
            public sealed class ResolvedEvent(ExpressionSyntax instance)
            {
                public EventRecord Event { get; }
                    = new(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            instance,
                            IdentifierName("Event")));

                public EventRecord OriginalEvent { get; }
                    = new(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            instance,
                            IdentifierName("OriginalEvent")));
            }

            public sealed class EventRecord(ExpressionSyntax instance)
            {
                public MemberAccessExpressionSyntax EventStreamId { get; }
                    = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        instance,
                        IdentifierName("EventStreamId"));

                public Uuid EventId { get; }
                    = new(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            instance,
                            IdentifierName("EventId")));

                public StreamPosition EventNumber { get; }
                    = new(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            instance,
                            IdentifierName("EventNumber")));

                public MemberAccessExpressionSyntax Data { get; }
                    = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        instance,
                        IdentifierName("Data"));

                public MemberAccessExpressionSyntax DataAsSpan
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Data,
                        IdentifierName("Span"));

                public MemberAccessExpressionSyntax Metadata { get; }
                    = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        instance,
                        IdentifierName("Metadata"));
                
                public MemberAccessExpressionSyntax MetadataAsSpan
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Metadata,
                        IdentifierName("Span"));

                public MemberAccessExpressionSyntax Created { get; }
                    = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        instance,
                        IdentifierName("Created"));
            }

            public sealed class Uuid(ExpressionSyntax instance)
            {
                public InvocationExpressionSyntax ToGuid { get; }
                    = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            instance,
                            IdentifierName("ToGuid")));
            }

            public sealed class StreamPosition(ExpressionSyntax instance)
            {
                public InvocationExpressionSyntax ToUInt64 { get; }
                    = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            instance,
                            IdentifierName("ToUInt64")));
            }
        }
    }
}