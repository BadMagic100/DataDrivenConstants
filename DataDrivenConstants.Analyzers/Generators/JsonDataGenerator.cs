using DataDrivenConstants.Util;
using DotNet.Globbing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataDrivenConstants.Generators;

[Generator(LanguageNames.CSharp)]
public class JsonDataGenerator : IIncrementalGenerator
{
    private record JsonDataProperties(
        string TargetNamespace,
        string Accessiblity,
        string TargetClass,
        Location Location,
        string ValuePath,
        CacheableList<string> FileGlobs);
    private record JsonGeneratorTarget(
        string TargetNamespace,
        string Accessibility,
        string TargetClass,
        Location Location,
        CacheableList<(string name, string value)> Members);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<JsonDataProperties> jsonDataMarkers = context.SyntaxProvider
            .ForAttributeWithMetadataName(JsonDataAttributeGenerator.AttributeFullName,
                predicate: CommonPredicates.IsStaticPartialClassDeclaration,
                transform: ExtractJsonDataProperties
            );

        IncrementalValuesProvider<(string path, string content)> jsonFiles = context.AdditionalTextsProvider
            .Select(GetNormalizedPathAndContent)
            .Where(x => x.Item2 != null)!;

        IncrementalValuesProvider<JsonGeneratorTarget> generatorTargets = jsonDataMarkers
            .Combine(jsonFiles.Collect())
            .Select(GatherMembers);

        context.RegisterSourceOutput(generatorTargets, EmitSources);
    }

    private JsonDataProperties ExtractJsonDataProperties(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        AttributeData attr = ctx.Attributes[0];

        TextSpan location = ((ClassDeclarationSyntax)ctx.TargetNode).Identifier.Span;
        string accessibility = SyntaxFacts.GetText(ctx.TargetSymbol.DeclaredAccessibility);
        string targetNamespace = ctx.TargetSymbol.ContainingNamespace.ToDisplayString();
        string target = ctx.TargetSymbol.Name;
        string valuePath = (string?)attr.ConstructorArguments[0].Value ?? "";
        CacheableList<string> globs;

        if (attr.ConstructorArguments[1].IsNull)
        {
            globs = new CacheableList<string>([]);
        }
        else
        {
            ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>(attr.ConstructorArguments[1].Values.Length);
            foreach (TypedConstant val in attr.ConstructorArguments[1].Values)
            {
                if (val.IsNull)
                {
                    continue;
                }
                builder.Add((string)val.Value!);
            }
            globs = new CacheableList<string>(builder.ToImmutableArray());
        }

        return new JsonDataProperties(targetNamespace, accessibility, target, Location.Create(ctx.TargetNode.SyntaxTree, location), valuePath, globs);
    }

    private (string, string?) GetNormalizedPathAndContent(AdditionalText text, CancellationToken ct)
    {
        return (text.Path.Replace('\\', '/'), text.GetText(ct)?.ToString());
    }

    private JsonGeneratorTarget GatherMembers((JsonDataProperties, ImmutableArray<(string, string)>) pair, CancellationToken ct)
    {
        (JsonDataProperties props, ImmutableArray<(string, string)> texts) = pair;
        List<Glob> globs = [.. props.FileGlobs.Select(Glob.Parse)];

        ImmutableArray<(string, string)>.Builder builder = ImmutableArray.CreateBuilder<(string, string)>();
        foreach ((string path, string content) in texts)
        {
            foreach (Glob glob in globs)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                if (glob.IsMatch(path))
                {
                    JContainer? container = JsonConvert.DeserializeObject<JContainer>(content);
                    if (container == null)
                    {
                        continue;
                    }

                    IEnumerable<string> values;
                    // support ~ (keys) operator from JSONPath Plus at end of path.
                    if (props.ValuePath[^1] == '~')
                    {
                        values = container.SelectTokens(props.ValuePath[0..^1])
                            .Select(t => t.Parent)
                            .Where(t => t?.Type == JTokenType.Property)
                            .Select(t => (t as JProperty)!.Name);
                    }
                    else
                    {
                        values = container.SelectTokens(props.ValuePath)
                            .Where(t => t.Type == JTokenType.String)
                            .Select(t => t.Value<string>()!);
                    }

                    builder.AddRange(values.Select(v => (Renamer.GetSafeName(v), v)));
                }
            }
        }
        builder.Sort();

        return new JsonGeneratorTarget(
            props.TargetNamespace,
            props.Accessiblity,
            props.TargetClass,
            props.Location,
            new CacheableList<(string name, string value)>(builder.ToImmutableArray())
        );
    }

    private void EmitSources(SourceProductionContext ctx, JsonGeneratorTarget target)
    {
        if (target.Members.Count == 0)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(Diagnostics.NoMembersFound, target.Location));
            return;
        }

        StringBuilder source = new($$"""
            namespace {{target.TargetNamespace}}
            {
                {{target.Accessibility}} static partial class {{target.TargetClass}}
                {

            """);

        foreach ((string name, string value) in target.Members)
        {
            SyntaxToken tok = SyntaxFactory.Literal(value);
            source.AppendLine($$"""
                    public const string {{name}} = {{tok}};
            """);
        }

        source.AppendLine("""
                }
            }
            """);
        ctx.AddSource(target.TargetClass + ".g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }
}
