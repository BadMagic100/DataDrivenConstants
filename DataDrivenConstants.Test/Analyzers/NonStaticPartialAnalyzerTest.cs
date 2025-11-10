using Microsoft.CodeAnalysis.Testing;
using VerifyCS = DataDrivenConstants.Test.Verifiers.CodeFixVerifierWithMarkers<
    DataDrivenConstants.Analyzers.NonStaticPartialAnalyzer,
    DataDrivenConstants.Fixes.NonStaticPartialFixes>;

namespace DataDrivenConstants.Test.Analyzers;

public class NonStaticPartialAnalyzerTest
{
    [Fact]
    public async Task EmptySourceReportsNoDiagnostic()
    {
        string source = "";
        await VerifyCS.VerifyCodeFixAsync(source, [], source);
    }

    [Fact]
    public async Task StaticPartialClassReportsNoDiagnostic()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.ReplacementRule("", "")]
            public static partial class MyClass {}
            """;

        await VerifyCS.VerifyCodeFixAsync(source, [], source);
    }

    [Fact]
    public async Task StaticClassOnlyReportsDiagnostic()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.ReplacementRule("", "")]
            public static class {|#0:MyClass|} {}
            """;
        DiagnosticResult expectedDiagnostic = new DiagnosticResult(Diagnostics.AttributeOnNonStaticPartialClass)
            .WithLocation(0)
            .WithArguments("MyClass");
        string fixSource = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.ReplacementRule("", "")]
            public static partial class MyClass {}
            """;

        await VerifyCS.VerifyCodeFixAsync(source, [expectedDiagnostic], fixSource);
    }

    [Fact]
    public async Task PartialClassOnlyReportsDiagnostic()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.ReplacementRule("", "")]
            public partial class {|#0:MyClass|} {}
            """;
        DiagnosticResult expectedDiagnostic = new DiagnosticResult(Diagnostics.AttributeOnNonStaticPartialClass)
            .WithLocation(0)
            .WithArguments("MyClass");
        string fixSource = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.ReplacementRule("", "")]
            public static partial class MyClass {}
            """;

        await VerifyCS.VerifyCodeFixAsync(source, [expectedDiagnostic], fixSource);
    }
}
