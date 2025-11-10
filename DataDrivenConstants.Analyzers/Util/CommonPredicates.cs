using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace DataDrivenConstants.Util;

internal static class CommonPredicates
{
    public static bool IsStaticPartialClassDeclaration(SyntaxNode node, CancellationToken ct)
    {
        if (node is not ClassDeclarationSyntax cd)
        {
            return false;
        }
        bool isStatic = false;
        bool isPartial = false;
        foreach (SyntaxToken t in cd.Modifiers)
        {
            if (t.IsKind(SyntaxKind.StaticKeyword))
            {
                isStatic = true;
            }

            if (t.IsKind(SyntaxKind.PartialKeyword))
            {
                isPartial = true;
            }
        }
        return isStatic && isPartial;
    }
}
