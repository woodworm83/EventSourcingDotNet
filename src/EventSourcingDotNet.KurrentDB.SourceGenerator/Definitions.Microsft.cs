using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Definitions
{
    public static class Microsoft
    {
        public static NameSyntax Namespace { get; }
            = SyntaxFactory.IdentifierName("global::Microsoft");

        public static class Extensions
        {
            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static NameSyntax Namespace { get; }
                = SyntaxFactory.QualifiedName(
                    Definitions.Microsoft.Namespace,
                    SyntaxFactory.IdentifierName("Extensions"));

            public static class DependencyInjection
            {
                [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
                public static NameSyntax Namespace { get; }
                    = SyntaxFactory.QualifiedName(
                        Extensions.Namespace,
                        SyntaxFactory.IdentifierName("DependencyInjection"));

                [SuppressMessage("ReSharper", "InconsistentNaming")]
                public static class IServiceCollection
                {
                    public static NameSyntax Type { get; }
                        = SyntaxFactory.QualifiedName(
                            Namespace,
                            SyntaxFactory.IdentifierName("IServiceCollection"));
                }

                public static class ServiceCollectionServiceExtensions
                {
                    public static NameSyntax Type { get; }
                        = SyntaxFactory.QualifiedName(
                            Namespace,
                            SyntaxFactory.IdentifierName("ServiceCollectionServiceExtensions"));

                    public static NameSyntax AddSingletonMethod { get; }
                        = SyntaxFactory.QualifiedName(
                            Type,
                            SyntaxFactory.IdentifierName("AddSingleton"));

                    public static NameSyntax GenericAddSingletonMethod(params TypeSyntax[] arguments)
                        => SyntaxFactory.QualifiedName(
                            Type,
                            SyntaxFactory
                                .GenericName("AddSingleton")
                                .AddTypeArgumentListArguments(arguments));
                }
            }
        }
    }
}