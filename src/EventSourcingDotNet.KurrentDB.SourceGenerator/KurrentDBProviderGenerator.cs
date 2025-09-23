using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

[Generator]
public sealed class KurrentDBProviderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, Compilation compilation)
    {
        context.AddSource(
            "KurrentDBProvider.g",
            SourceText.From(
                CompilationUnit()
                    .AddUsings(UsingDirective(Definitions.Microsoft.Extensions.DependencyInjection.Namespace))
                    .AddMembers(CreateNamespace())
                    .NormalizeWhitespace(elasticTrivia: true)
                    .ToFullString(),
                Encoding.UTF8));
    }

    private static BaseNamespaceDeclarationSyntax CreateNamespace()
        => FileScopedNamespaceDeclaration(IdentifierName("EventSourcingDotNet.KurrentDB"))
            .WithLeadingTrivia(
                Trivia(
                    NullableDirectiveTrivia(
                        Token(SyntaxKind.EnableKeyword),
                        isActive: true)))
            .AddMembers(CreateKurrentDBProviderClass())
            .NormalizeWhitespace();

    private static MemberDeclarationSyntax CreateKurrentDBProviderClass()
        => ClassDeclaration("KurrentDBProvider")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword))
            .AddBaseListTypes(SimpleBaseType(Definitions.EventSourcingDotNet.IEventStoreProvider.Type))
            .AddParameterListParameters(
                Parameter(Identifier("clientSettings"))
                    .WithType(Definitions.KurrentDB.Client.KurrentDBClientSettings.Type),
                Parameter(Identifier("serializerContext"))
                    .WithType(Definitions.System.Text.Json.Serialization.JsonSerializerContext.Type))
            .AddMembers(
                CreateRegisterServicesMethod(),
                CreateEventSerializerFactoryMethod());

    private static MemberDeclarationSyntax CreateRegisterServicesMethod()
        => MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("RegisterServices"))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("services"))
                    .WithType(Definitions.Microsoft.Extensions.DependencyInjection.IServiceCollection.Type))
            .AddBodyStatements(
                ExpressionStatement(
                    InvocationExpression(
                            Definitions.EventSourcingDotNet.KurrentDB.RegistrationExtensions.AddKurrentDBServicesMethod)
                        .AddArgumentListArguments(
                            Argument(IdentifierName("services")),
                            Argument(IdentifierName("clientSettings")))),
                ExpressionStatement(
                    InvocationExpression(
                            Definitions.Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions
                                .GenericAddSingletonMethod(
                                    Definitions.EventSourcingDotNet.KurrentDB.IEventSerializer.Type))
                        .AddArgumentListArguments(
                            Argument(IdentifierName("services")),
                            Argument(IdentifierName("CreateEventSerializer")))));

    public static MethodDeclarationSyntax CreateEventSerializerFactoryMethod()
        => MethodDeclaration(
                Definitions.EventSourcingDotNet.KurrentDB.EventSerializer.Type,
                "CreateEventSerializer")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("serviceProvider"))
                    .WithType(Definitions.System.IServiceProvider.Type))
            .WithExpressionBody(
                ArrowExpressionClause(
                    ObjectCreationExpression(Definitions.EventSourcingDotNet.KurrentDB.EventSerializer.Type)
                        .AddArgumentListArguments(Argument(IdentifierName("serializerContext")))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
}