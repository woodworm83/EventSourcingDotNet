using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Instances
{
    public static class EventSourcingDotNet
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members")]
        public static class KurrentDB
        {
            public sealed class EventMetadata(ExpressionSyntax instance)
            {
                public MemberAccessExpressionSyntax AggregateId { get; }
                = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    instance,
                    IdentifierName("AggregateId"));
                
                public MemberAccessExpressionSyntax CorrelationId { get; }
                = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    instance,
                    IdentifierName("CorrelationId"));
                
                public MemberAccessExpressionSyntax CausationId { get; }
                = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    instance,
                    IdentifierName("CausationId"));
            }
        }
    }
}