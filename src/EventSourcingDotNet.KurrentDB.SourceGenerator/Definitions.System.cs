using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Definitions
{
    public static class System
    {
        private static NameSyntax Namespace { get; }
            = SyntaxFactory.IdentifierName("global::System");

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static class IServiceProvider
        {
            public static NameSyntax Type { get; }
                = SyntaxFactory.QualifiedName(
                    Namespace,
                    SyntaxFactory.IdentifierName("IServiceProvider"));
        }

        public static class Diagnostics
        {
            private static NameSyntax Namespace { get; }
                = SyntaxFactory.QualifiedName(
                    Definitions.System.Namespace,
                    SyntaxFactory.IdentifierName("Diagnostics"));

            [SuppressMessage(
                "Major Code Smell",
                "S2166:Classes named like \"Exception\" should extend \"Exception\" or a subclass")]
            public static class UnreachableException
            {
                public static NameSyntax Type { get; } =
                    SyntaxFactory.QualifiedName(
                        Namespace,
                        SyntaxFactory.IdentifierName("UnreachableException"));
            }
        }

        public static class Text
        {
            private static QualifiedNameSyntax Namespace { get; }
                = SyntaxFactory.QualifiedName(
                    Definitions.System.Namespace,
                    SyntaxFactory.IdentifierName("Text"));

            public static class Json
            {
                private static QualifiedNameSyntax Namespace { get; }
                    = SyntaxFactory.QualifiedName(
                        Definitions.System.Text.Namespace,
                        SyntaxFactory.IdentifierName("Json"));

                public static class JsonSerializer
                {
                    public static NameSyntax Type { get; }
                        = SyntaxFactory.QualifiedName(
                            Namespace,
                            SyntaxFactory.IdentifierName("JsonSerializer"));

                    public static NameSyntax SerializeToUtf8BytesMethod { get; }
                        = SyntaxFactory.QualifiedName(
                            Type,
                            SyntaxFactory.IdentifierName("SerializeToUtf8Bytes"));
                }

                public static class JsonSerializerOptions
                {
                    public static NameSyntax Type { get; }
                        = SyntaxFactory.QualifiedName(
                            Namespace,
                            SyntaxFactory.IdentifierName("JsonSerializerOptions"));
                }

                public static class Serialization
                {
                    private static NameSyntax Namespace { get; }
                        = SyntaxFactory.QualifiedName(
                            Definitions.System.Text.Json.Namespace,
                            SyntaxFactory.IdentifierName("Serialization"));

                    public static class JsonSerializerContext
                    {
                        public static NameSyntax Type { get; }
                            = SyntaxFactory.QualifiedName(
                                Namespace,
                                SyntaxFactory.IdentifierName("JsonSerializerContext"));

                        public static SimpleNameSyntax GetTypeInfoMethod { get; }
                            = SyntaxFactory.IdentifierName("GetTypeInfo");
                    }

                    public static class Metadata
                    {
                        public static NameSyntax Namespace { get; }
                            = SyntaxFactory.QualifiedName(
                                Definitions.System.Text.Json.Serialization.Namespace,
                                SyntaxFactory.IdentifierName("Metadata"));

                        public static class JsonTypeInfo
                        {
                            public static NameSyntax Type { get; }
                                = SyntaxFactory.QualifiedName(
                                    Namespace,
                                    SyntaxFactory.IdentifierName("JsonTypeInfo"));
                        }
                    }
                }
            }
        }
    }
}