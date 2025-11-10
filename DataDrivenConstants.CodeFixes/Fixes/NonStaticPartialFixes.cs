using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataDrivenConstants.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class NonStaticPartialFixes : CodeFixProvider
{
    public const string FixKeyMakeStaticPartial = "MakeStaticPartial";

    public override ImmutableArray<string> FixableDiagnosticIds => [Diagnostics.AttributeOnNonStaticPartialClass.Id];

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics.First();
        TextSpan span = diagnostic.Location.SourceSpan;
        ClassDeclarationSyntax? cd = root.FindToken(span.Start).Parent?.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (cd == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make static partial",
                createChangedDocument: c => MakeStaticPartialAsync(
                    document: context.Document,
                    cd: cd,
                    ct: c
                ),
                equivalenceKey: FixKeyMakeStaticPartial
            ),
            diagnostic
        );
    }

    private async Task<Document> MakeStaticPartialAsync(
        Document document,
        ClassDeclarationSyntax cd,
        CancellationToken ct
    )
    {
        DocumentEditor editor = await DocumentEditor.CreateAsync(document, ct);
        editor.SetModifiers(cd, editor.Generator.GetModifiers(cd).WithIsStatic(true).WithPartial(true));

        Document newDoc = editor.GetChangedDocument();
        return newDoc;
    }
}
