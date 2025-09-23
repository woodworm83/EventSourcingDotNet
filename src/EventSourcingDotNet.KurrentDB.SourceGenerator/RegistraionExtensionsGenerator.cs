using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

[Generator]
public sealed class RegistraionExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, Compilation compilation)
    {
        context.AddSource(
            "KurrentDBRegistrationExtensions.g",
            SourceText.From(
                SyntaxFactory
                    .CompilationUnit()
                    .AddUsings(
                        SyntaxFactory.UsingDirective(Definitions.Microsoft.Extensions.DependencyInjection.Namespace))
                    .AddMembers(CreateNamespace())
                    .NormalizeWhitespace(elasticTrivia: true)
                    .ToFullString(),
                Encoding.UTF8));
    }

    private static BaseNamespaceDeclarationSyntax CreateNamespace()
        => SyntaxFactory
            .FileScopedNamespaceDeclaration(SyntaxFactory.IdentifierName("EventSourcingDotNet"))
            .WithLeadingTrivia(
                SyntaxFactory
                    .Trivia(
                        SyntaxFactory.NullableDirectiveTrivia(
                            SyntaxFactory.Token(SyntaxKind.EnableKeyword),
                            isActive: true)))
            .AddMembers(CreateRegistrationExtensionsClass())
            .NormalizeWhitespace();

    private static ClassDeclarationSyntax CreateRegistrationExtensionsClass()
        => SyntaxFactory
            .ClassDeclaration("KurrentDBRegistrationExtensions")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .AddMembers(CreateUseKurrentDBMethod());

    private static MemberDeclarationSyntax CreateUseKurrentDBMethod()
        => SyntaxFactory
            .MethodDeclaration(
                Definitions.EventSourcingDotNet.EventSourcingBuilder.Type,
                "UseKurrentDB")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(
                SyntaxFactory
                    .Parameter(SyntaxFactory.Identifier("builder"))
                    .WithType(Definitions.EventSourcingDotNet.EventSourcingBuilder.Type)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.ThisKeyword)),
                SyntaxFactory
                    .Parameter(SyntaxFactory.Identifier("clientSettings"))
                    .WithType(Definitions.KurrentDB.Client.KurrentDBClientSettings.Type),
                SyntaxFactory
                    .Parameter(SyntaxFactory.Identifier("serializerContext"))
                    .WithType(Definitions.System.Text.Json.Serialization.JsonSerializerContext.Type))
            .AddBodyStatements(
                SyntaxFactory.ExpressionStatement(
                    Definitions.EventSourcingDotNet.EventSourcingBuilder.UseEventStoreProvider(
                        SyntaxFactory.IdentifierName("builder"),
                        Definitions.EventSourcingDotNet.KurrentDB.KurrentDBProvider.New(
                            SyntaxFactory.IdentifierName("clientSettings"),
                            SyntaxFactory.IdentifierName("serializerContext")))),
                SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("builder")));
}