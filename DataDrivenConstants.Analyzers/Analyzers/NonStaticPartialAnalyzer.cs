
using DataDrivenConstants.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DataDrivenConstants.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NonStaticPartialAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostics.AttributeOnNonStaticPartialClass);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (CommonPredicates.IsStaticPartialClassDeclaration(context.Node, context.CancellationToken))
        {
            return;
        }

        ClassDeclarationSyntax cd = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol? type = context.SemanticModel.GetDeclaredSymbol(cd);
        if (type == null)
        {
            return;
        }

        foreach (AttributeData attr in type.GetAttributes())
        {
            if (attr.AttributeClass?.ContainingNamespace.ToDisplayString() == "DataDrivenConstants.Marker")
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.AttributeOnNonStaticPartialClass,
                    cd.Identifier.GetLocation(),
                    type.Name
                ));
                break;
            }
        }
    }
}
