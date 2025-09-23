using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EventSourcingDotNet.KurrentDB.SourceGenerator;

public static partial class Definitions
{
    // ReSharper disable once InconsistentNaming
    public static partial class KurrentDB
    {
        private static NameSyntax Namespace { get; }
            = IdentifierName("global::KurrentDB");

    }
}