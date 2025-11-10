using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace DataDrivenConstants.Test.Verifiers;
internal class SourceGeneratorTestWithMarkers<TGen> : CSharpSourceGeneratorTest<TGen, DefaultVerifier>
    where TGen : IIncrementalGenerator, new()
{
    protected override IEnumerable<Type> GetSourceGenerators() => [
        .. MarkerAttributeInjector.MarkerAttributeTypes,
        typeof(TGen)
    ];

    public SourceGeneratorTestWithMarkers() : base()
    {
        MarkerAttributeInjector.ApplySourceGen(TestState);
    }
}
