using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
[SuppressMessage(
    "Critical Code Smell",
    "S3218:Inner class members should not shadow outer class \"static\" or type members")]
public static partial class Definitions
{
    public static class EventSourcingDotNet
    {
        public static NameSyntax Namespace { get; }
            = SyntaxFactory.IdentifierName("global::EventSourcingDotNet");

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static class IResolvedEvent
        {
            public static NameSyntax Type { get; }
                = SyntaxFactory.QualifiedName(
                    Namespace,
                    SyntaxFactory.IdentifierName("IResolvedEvent"));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static class IEventStoreProvider
        {
            public static NameSyntax Type { get; }
                = SyntaxFactory.QualifiedName(
                    Namespace,
                    SyntaxFactory.IdentifierName("IEventStoreProvider"));
        }

        public static class EventSourcingBuilder
        {
            public static NameSyntax Type { get; }
                = SyntaxFactory.QualifiedName(
                    Namespace,
                    SyntaxFactory.IdentifierName("EventSourcingBuilder"));

            public static SimpleNameSyntax UseEventStoreProviderMethod { get; }
                = SyntaxFactory.IdentifierName("UseEventStoreProvider");
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static class KurrentDB
        {
            public static NameSyntax Namespace { get; }
                = SyntaxFactory.QualifiedName(
                    Definitions.EventSourcingDotNet.Namespace,
                    SyntaxFactory.IdentifierName("KurrentDB"));

            [SuppressMessage("ReSharper", "UnusedType.Global")]
            public static class IEventSerializer
            {
                public static NameSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(
                        Namespace,
                        SyntaxFactory.IdentifierName("IEventSerializer"));
            }

            public static class RegistrationExtensions
            {
                public static NameSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(
                        Namespace,
                        SyntaxFactory.IdentifierName("RegistrationExtensions"));

                public static NameSyntax AddKurrentDBServicesMethod { get; }
                    = SyntaxFactory.QualifiedName(
                        Type,
                        SyntaxFactory.IdentifierName("AddKurrentDBServices"));
            }

            public static class KurrentDBProvider
            {
                public static NameSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(
                        Namespace,
                        SyntaxFactory.IdentifierName("KurrentDBProvider"));
            }

            public static class EventSerializer
            {
                public static NameSyntax Type { get; }
                    = SyntaxFactory.QualifiedName(Namespace, SyntaxFactory.IdentifierName("EventSerializer"));
            }
        }
    }
}