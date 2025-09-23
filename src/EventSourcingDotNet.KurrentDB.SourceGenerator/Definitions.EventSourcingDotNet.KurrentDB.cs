using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable MemberCanBePrivate.Global

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Definitions
{
    public static partial class EventSourcingDotNet
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static class KurrentDB
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static NameSyntax Namespace { get; }
                = QualifiedName(
                    Definitions.EventSourcingDotNet.Namespace,
                    IdentifierName("KurrentDB"));

            // ReSharper disable once InconsistentNaming
            public static class IEventSerializer
            {
                public static NameSyntax Type { get; }
                    = QualifiedName(Namespace, IdentifierName("IEventSerializer"));
            }

            public static class RegistrationExtensions
            {
                public static NameSyntax Type { get; }
                    = QualifiedName(Namespace, IdentifierName("RegistrationExtensions"));

                public static NameSyntax AddKurrentDBServicesMethod { get; }
                    = QualifiedName(Type, IdentifierName("AddKurrentDBServices"));
            }

            public static class KurrentDBProvider
            {
                public static NameSyntax Type { get; }
                    = QualifiedName(Namespace, IdentifierName("KurrentDBProvider"));

                public static ObjectCreationExpressionSyntax New(
                    ExpressionSyntax clientSettings,
                    ExpressionSyntax serializerContext)
                    => ObjectCreationExpression(Type)
                        .AddArgumentListArguments(
                            Argument(clientSettings),
                            Argument(serializerContext));
            }

            public static class EventSerializer
            {
                public static NameSyntax Type { get; }
                    = QualifiedName(Namespace, IdentifierName("EventSerializer"));
            }

            public static class EventMetadata
            {
                public static TypeSyntax GenericType(TypeSyntax aggregateIdType)
                    => QualifiedName(
                        Namespace,
                        GenericName("EventMetadata")
                            .AddTypeArgumentListArguments(aggregateIdType));

                public static ObjectCreationExpressionSyntax GenericNew(
                    TypeSyntax aggregateIdType,
                    ExpressionSyntax aggregateId,
                    ExpressionSyntax correlationId,
                    ExpressionSyntax causationId)
                    => ObjectCreationExpression(GenericType(aggregateIdType))
                        .AddArgumentListArguments(
                            Argument(aggregateId),
                            Argument(correlationId),
                            Argument(causationId));
            }
        }
    }
}