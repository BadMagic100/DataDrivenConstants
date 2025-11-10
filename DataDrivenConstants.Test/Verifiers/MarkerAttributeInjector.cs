using DataDrivenConstants.Generators;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;

namespace DataDrivenConstants.Test.Verifiers;
internal static class MarkerAttributeInjector
{
    public static readonly ImmutableArray<(Type generatorType, string fileName, string source)> MarkerGeneratedSources = [
        (
            typeof(JsonDataAttributeGenerator),
            JsonDataAttributeGenerator.AttributeFileName,
            JsonDataAttributeGenerator.AttributeSource
        ),
        (
            typeof(ReplacementRuleAttributeGenerator),
            ReplacementRuleAttributeGenerator.AttributeFileName,
            ReplacementRuleAttributeGenerator.AttributeSource
        )
    ];

    public static readonly ImmutableArray<Type> MarkerAttributeTypes = [
        ..MarkerGeneratedSources.Select(m => m.generatorType),
    ];

    public static void ApplySourceGen(SolutionState state)
    {
        foreach ((Type generatorType, string fileName, string source) source in MarkerGeneratedSources)
        {
            state.GeneratedSources.Add(source);
        }
    }
}
