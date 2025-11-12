
using DataDrivenConstants.Generators;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataDrivenConstants.Util;

internal record ReplacementRule(string OldString, string NewString, bool Regex);

internal static class ReplacementRuleFetcherExtensions
{
    /// <summary>
    /// Gets replacement rules off a class declaration
    /// </summary>
    public static ImmutableArray<ReplacementRule> GetReplacements(this ISymbol classDeclaration, SemanticModel semanticModel)
    {
        INamedTypeSymbol? attrType = semanticModel.Compilation.GetTypeByMetadataName(ReplacementRuleAttributeGenerator.AttributeFullName);
        if (attrType == null)
        {
            return ImmutableArray<ReplacementRule>.Empty;
        }
        return classDeclaration.GetAttributes()
            .Where(a => a.AttributeClass?.Equals(attrType, SymbolEqualityComparer.Default) == true
                        && !a.ConstructorArguments[0].IsNull
                        && !a.ConstructorArguments[1].IsNull)
            .Select(a =>
            {
                string oldStr = (string)a.ConstructorArguments[0].Value!;
                string newStr = (string)a.ConstructorArguments[1].Value!;
                bool regex = (int)a.ConstructorArguments[2].Value! == 1;
                return new ReplacementRule(oldStr, newStr, regex);
            }).ToImmutableArray();
    }

    public static string ApplyReplacements(this string s, IEnumerable<ReplacementRule> replacements)
    {
        string workingValue = s;
        foreach (ReplacementRule replacement in replacements)
        {
            if (replacement.Regex)
            {
                workingValue = Regex.Replace(workingValue, replacement.OldString, replacement.NewString);
            }
            else
            {
                workingValue = workingValue.Replace(replacement.OldString, replacement.NewString);
            }
        }
        return workingValue;
    }
}
