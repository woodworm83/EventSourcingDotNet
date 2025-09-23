using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Definitions
{
    public static partial class System
    {
        private static NameSyntax Namespace { get; }
            = IdentifierName("global::System");

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static class IServiceProvider
        {
            public static NameSyntax Type { get; }
                = QualifiedName(
                    Namespace,
                    IdentifierName("IServiceProvider"));
        }

        public static class ReadOnlyMemory
        {
            public static NameSyntax GenericType(TypeSyntax type)
                => QualifiedName(
                    Namespace,
                    GenericName("ReadOnlyMemory")
                        .AddTypeArgumentListArguments(type));
        }

        public static class Object
        {
            public static InvocationExpressionSyntax GetType(ExpressionSyntax instance)
                => InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        instance,
                        IdentifierName("GetType")));
        }

        public static class Diagnostics
        {
            private static NameSyntax Namespace { get; }
                = QualifiedName(
                    Definitions.System.Namespace,
                    IdentifierName("Diagnostics"));

            [SuppressMessage(
                "Major Code Smell",
                "S2166:Classes named like \"Exception\" should extend \"Exception\" or a subclass")]
            public static class UnreachableException
            {
                public static NameSyntax Type { get; } =
                    QualifiedName(
                        Namespace,
                        IdentifierName("UnreachableException"));

                public static ThrowExpressionSyntax Throw { get; }
                    = ThrowExpression(ObjectCreationExpression(Type).AddArgumentListArguments());
            }
        }

        public static partial class Text
        {
            private static QualifiedNameSyntax Namespace { get; }
                = QualifiedName(
                    Definitions.System.Namespace,
                    IdentifierName("Text"));
        }

        public static class Threading
        {
            private static NameSyntax Namespace { get; }
                = QualifiedName(Definitions.System.Namespace, IdentifierName("Threading"));

            public static class Tasks
            {
                private static NameSyntax Namespace { get; }
                    = QualifiedName(Definitions.System.Threading.Namespace, IdentifierName("Tasks"));

                public static class ValueTask
                {
                    public static NameSyntax Type { get; }
                        = QualifiedName(Namespace, IdentifierName("ValueTask"));

                    public static NameSyntax GenericType(TypeSyntax type)
                        => QualifiedName(Namespace, GenericName("ValueTask").AddTypeArgumentListArguments(type));

                    public static ObjectCreationExpressionSyntax GenericNew(
                        TypeSyntax type,
                        ExpressionSyntax value)
                        => ObjectCreationExpression(GenericType(type))
                            .AddArgumentListArguments(Argument(value));

                    public static ExpressionSyntax ConfigureAwait(ExpressionSyntax task, bool continueOnCapturedContext)
                        => InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    task,
                                    IdentifierName("ConfigureAwait")))
                            .AddArgumentListArguments(
                                Argument(
                                    LiteralExpression(
                                        continueOnCapturedContext
                                            ? SyntaxKind.TrueLiteralExpression
                                            : SyntaxKind.FalseLiteralExpression)));
                }
            }
        }
    }
}