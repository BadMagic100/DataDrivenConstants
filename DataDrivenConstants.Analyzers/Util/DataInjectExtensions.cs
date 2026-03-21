using DataDrivenConstants.Generators;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DataDrivenConstants.Util;

internal record DataInjectProperties(
    bool IsStringParameter,
    string MethodName,
    string ReturnType,
    Location Location,
    int InjectedParameterIndex,
    CacheableList<(string type, string name)> CopiedParameters);

internal static class DataInjectExtensions
{
    public static CacheableList<DataInjectProperties> GetInjections(this ISymbol symbol, SemanticModel semanticModel)
    {
        if (symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return CacheableList.Of<DataInjectProperties>();
        }

        INamedTypeSymbol? attrType = semanticModel.Compilation.GetTypeByMetadataName(DataInjectAttributeGenerator.AttributeFullName);
        if (attrType == null)
        {
            return CacheableList.Of<DataInjectProperties>();
        }

        ImmutableArray<DataInjectProperties>.Builder builder = ImmutableArray.CreateBuilder<DataInjectProperties>();
        foreach (IMethodSymbol method in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            foreach (IParameterSymbol parameter in method.Parameters)
            {
                if (parameter.GetAttributes().Any(attr => attr.AttributeClass?.Equals(attrType, SymbolEqualityComparer.Default) == true))
                {
                    builder.Add(ExtractDataInjectProperties(namedTypeSymbol, method, parameter));
                }
            }
        }
        return CacheableList.Of(builder.ToImmutable());
    }

    private static DataInjectProperties ExtractDataInjectProperties(INamedTypeSymbol containingType, IMethodSymbol method, IParameterSymbol param)
    {
        bool isString = param.Type.SpecialType == SpecialType.System_String;
        int index = 0;
        ImmutableArray<(string, string)>.Builder builder = ImmutableArray.CreateBuilder<(string, string)>(method.Parameters.Length - 1);
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            IParameterSymbol current = method.Parameters[i];
            if (current.Equals(param, SymbolEqualityComparer.Default))
            {
                index = i;
            }
            else
            {
                builder.Add((current.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), current.Name));
            }
        }
        return new DataInjectProperties(
            isString,
            method.Name,
            method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            param.Locations[0],
            index,
            new CacheableList<(string declaration, string name)>(builder.ToImmutable())
        );
    }
}
