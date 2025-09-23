using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static class ConversionExtensions
{
    public static ObjectCreationExpressionSyntax ToAggregateVersion(
        this Instances.KurrentDB.Client.StreamPosition streamPosition)
        => ObjectCreationExpression(Definitions.EventSourcingDotNet.AggregateVersion.Type)
            .AddArgumentListArguments(
                Argument(
                    BinaryExpression(
                        SyntaxKind.AddExpression,
                        streamPosition.ToUInt64,
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1ul)))));

    public static ObjectCreationExpressionSyntax ToStreamPosition(
        this Instances.KurrentDB.Client.StreamPosition streamPosition)
        => ObjectCreationExpression(Definitions.EventSourcingDotNet.StreamPosition.Type)
            .AddArgumentListArguments(Argument(streamPosition.ToUInt64));
}