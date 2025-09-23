using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Definitions
{
    public static partial class System
    {
        public static partial class Text
        {
            public static class Json
            {
                private static QualifiedNameSyntax Namespace { get; }
                    = QualifiedName(
                        Definitions.System.Text.Namespace,
                        IdentifierName("Json"));

                public static class JsonSerializer
                {
                    public static NameSyntax Type { get; }
                        = QualifiedName(
                            Namespace,
                            IdentifierName("JsonSerializer"));

                    public static InvocationExpressionSyntax SerializeToUtf8Bytes(
                        ExpressionSyntax value,
                        ExpressionSyntax type,
                        ExpressionSyntax serializerContext)
                        => InvocationExpression(SerializeToUtf8BytesMethod)
                            .AddArgumentListArguments(
                                Argument(value),
                                Argument(type),
                                Argument(serializerContext));

                    public static NameSyntax SerializeToUtf8BytesMethod { get; } = QualifiedName(
                        Type,
                        IdentifierName("SerializeToUtf8Bytes"));

                    public static InvocationExpressionSyntax Deserialize(
                        TypeSyntax type,
                        ExpressionSyntax data,
                        ExpressionSyntax jsonTypeInfo)
                        => InvocationExpression(
                                QualifiedName(
                                    Type,
                                    GenericName("Deserialize")
                                        .AddTypeArgumentListArguments(type)))
                            .AddArgumentListArguments(
                                Argument(data),
                                Argument(jsonTypeInfo));

                    public static InvocationExpressionSyntax Deserialize(
                        TypeSyntax type,
                        ExpressionSyntax data)
                        => InvocationExpression(
                                QualifiedName(
                                    Type,
                                    GenericName("Deserialize")
                                        .AddTypeArgumentListArguments(type)))
                            .AddArgumentListArguments(Argument(data));

                    public static NameSyntax CreateDeserializeMethod(NameSyntax type)
                        => QualifiedName(
                            Type,
                            GenericName("Deserialize")
                                .AddTypeArgumentListArguments(type));
                }

                public static class JsonSerializerOptions
                {
                    public static NameSyntax Type { get; }
                        = QualifiedName(
                            Namespace,
                            IdentifierName("JsonSerializerOptions"));
                }

                [SuppressMessage("Major Code Smell", "S2166:Classes named like \"Exception\" should extend \"Exception\" or a subclass")]
                public static class JsonException
                {
                    public static TypeSyntax Type { get; }
                        = QualifiedName(
                            Namespace,
                            IdentifierName("JsonException"));
                }

                public static class Serialization
                {
                    private static NameSyntax Namespace { get; }
                        = QualifiedName(
                            Definitions.System.Text.Json.Namespace,
                            IdentifierName("Serialization"));

                    public static class JsonSerializerContext
                    {
                        public static NameSyntax Type { get; }
                            = QualifiedName(
                                Namespace,
                                IdentifierName("JsonSerializerContext"));

                        public static InvocationExpressionSyntax GetTypeInfo(
                            ExpressionSyntax jsonSerializerContextInstance,
                            ExpressionSyntax type)
                            => InvocationExpression(
                                    ConditionalAccessExpression(
                                        jsonSerializerContextInstance,
                                        MemberBindingExpression(
                                            IdentifierName("GetTypeInfo"))))
                                .AddArgumentListArguments(Argument(type));

                        public static ExpressionSyntax GetTypeInfoAs(
                            TypeSyntax type,
                            ExpressionSyntax jsonSerializerContextInstance)
                            => BinaryExpression(
                                SyntaxKind.AsExpression,
                                GetTypeInfo(
                                    jsonSerializerContextInstance,
                                    TypeOfExpression(type)),
                                Metadata.JsonTypeInfo.GenericType(type));
                    }

                    // ReSharper disable once InconsistentNaming
                    public static class IJsonTypeInfoResolver
                    {
                        public static NameSyntax Type { get; }
                            = QualifiedName(
                                Namespace,
                                IdentifierName("IJsonTypeInfoResolver"));

                        public static InvocationExpressionSyntax GetTypeInfo(
                            ExpressionSyntax type,
                            ExpressionSyntax jsonSerializerOptions)
                            => InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        Type,
                                        IdentifierName("GetTypeInfo")))
                                .AddArgumentListArguments(
                                    Argument(type),
                                    Argument(jsonSerializerOptions));
                    }

                    public static class Metadata
                    {
                        public static NameSyntax Namespace { get; }
                            = QualifiedName(
                                Serialization.Namespace,
                                IdentifierName("Metadata"));

                        public static class JsonTypeInfo
                        {
                            public static NameSyntax Type { get; }
                                = QualifiedName(
                                    Namespace,
                                    IdentifierName("JsonTypeInfo"));

                            public static TypeSyntax GenericType(TypeSyntax type)
                                => QualifiedName(
                                    Namespace,
                                    GenericName("JsonTypeInfo")
                                        .AddTypeArgumentListArguments(type));
                        }
                    }
                }
            }
        }
    }
}