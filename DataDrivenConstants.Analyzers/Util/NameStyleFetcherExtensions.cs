using DataDrivenConstants.Generators;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace DataDrivenConstants.Util;

internal enum NameStyle
{
    MinimalTransform,
    PascalCase,
    CamelCase,
    SnakeCase,
    UpperSnakeCase
}

internal static class NameStyleFetcherExtensions
{
    /// <summary>
    /// Gets the preferred name style off a class declaration
    /// </summary>
    public static NameStyle GetNameStyle(this ISymbol classDeclaration, SemanticModel semanticModel)
    {
        INamedTypeSymbol? attrType = semanticModel.Compilation.GetTypeByMetadataName(NameGenerationStyleAttributeGenerator.AttributeFullName);
        if (attrType == null)
        {
            return NameStyle.MinimalTransform;
        }
        return classDeclaration.GetAttributes()
            .Where(a => a.AttributeClass?.Equals(attrType, SymbolEqualityComparer.Default) == true
                        && !a.ConstructorArguments[0].IsNull)
            .Select(a =>
            {
                int value = (int)a.ConstructorArguments[0].Value!;
                return value switch
                {
                    0 => NameStyle.PascalCase,
                    1 => NameStyle.CamelCase,
                    2 => NameStyle.SnakeCase,
                    3 => NameStyle.UpperSnakeCase,
                    _ => NameStyle.MinimalTransform,
                };
            }).FirstOrDefault();
    }
}
