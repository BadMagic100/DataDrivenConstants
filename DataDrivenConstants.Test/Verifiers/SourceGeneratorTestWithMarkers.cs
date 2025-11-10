using DataDrivenConstants.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace DataDrivenConstants.Test.Verifiers;
internal class SourceGeneratorTestWithMarkers<TGen> : CSharpSourceGeneratorTest<TGen, DefaultVerifier>
    where TGen : IIncrementalGenerator, new()
{
    protected override IEnumerable<Type> GetSourceGenerators() => [
        typeof(JsonDataAttributeGenerator),
        typeof(TGen)
    ];

    public SourceGeneratorTestWithMarkers() : base()
    {
        TestState.GeneratedSources.Add((
            typeof(JsonDataAttributeGenerator),
            JsonDataAttributeGenerator.AttributeFileName,
            JsonDataAttributeGenerator.AttributeSource
        ));
    }
}
