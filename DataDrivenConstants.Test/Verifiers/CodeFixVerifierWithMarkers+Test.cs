using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace DataDrivenConstants.Test.Verifiers;
internal partial class CodeFixVerifierWithMarkers<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    private class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        protected override IEnumerable<Type> GetSourceGenerators() => MarkerAttributeInjector.MarkerAttributeTypes;

        public Test()
        {
            MarkerAttributeInjector.ApplySourceGen(TestState);
        }
    }
}
