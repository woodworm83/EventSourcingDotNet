using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Definitions
{
    // ReSharper disable once InconsistentNaming
    public static class KurrentDB
    {
        private static NameSyntax Namespace { get; }
            = SyntaxFactory.IdentifierName("global::KurrentDB");

        public static class Client
        {
            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            private static NameSyntax Namespace { get; }
                = SyntaxFactory.QualifiedName(
                    Definitions.KurrentDB.Namespace,
                    SyntaxFactory.IdentifierName("Client"));

            public static class ResolvedEvent
            {
                public static TypeSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(
                        Namespace,
                        SyntaxFactory.IdentifierName("ResolvedEvent"));
            }

            public static class EventData
            {
                public static NameSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(
                        Namespace,
                        SyntaxFactory.IdentifierName("EventData"));
            }

            public static class Uuid
            {
                public static NameSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(
                        Namespace,
                        SyntaxFactory.IdentifierName("Uuid"));
            }

            public static class KurrentDBClient
            {
                public static TypeSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(Namespace, SyntaxFactory.IdentifierName("KurrentDBClient"));
            }

            public static class KurrentDBClientSettings
            {
                public static NameSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(
                        Namespace,
                        SyntaxFactory.IdentifierName("KurrentDBClientSettings"));
            }
        }
    }
}