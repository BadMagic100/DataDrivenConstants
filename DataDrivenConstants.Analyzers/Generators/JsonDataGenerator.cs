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
        CacheableList<string> FileGlobs,
        CacheableList<ReplacementRule> ReplacementRules,
        CacheableList<DataInjectProperties> Injections);
    private record JsonGeneratorTarget(
        string TargetNamespace,
        string Accessibility,
        string TargetClass,
        Location Location,
        CacheableList<DataInjectProperties> Injections,
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

        CacheableList<ReplacementRule> replacements = ctx.TargetSymbol.GetReplacements(ctx.SemanticModel);
        CacheableList<DataInjectProperties> injections = ctx.TargetSymbol.GetInjections(ctx.SemanticModel);

        TextSpan location = ((ClassDeclarationSyntax)ctx.TargetNode).Identifier.Span;
        string accessibility = SyntaxFacts.GetText(ctx.TargetSymbol.DeclaredAccessibility);
        string targetNamespace = ctx.TargetSymbol.ContainingNamespace.ToDisplayString();
        string target = ctx.TargetSymbol.Name;
        string valuePath = (string?)attr.ConstructorArguments[0].Value ?? "";
        CacheableList<string> globs;

        if (attr.ConstructorArguments[1].IsNull)
        {
            globs = new CacheableList<string>(ImmutableArray<string>.Empty);
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
            globs = new CacheableList<string>(builder.ToImmutable());
        }

        return new JsonDataProperties(
            targetNamespace,
            accessibility,
            target,
            Location.Create(ctx.TargetNode.SyntaxTree, location),
            valuePath,
            globs,
            replacements,
            injections
        );
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

                    builder.AddRange(values.Select(v => (Renamer.GetSafeName(v, props.ReplacementRules), v)));
                }
            }
        }
        builder.Sort();

        return new JsonGeneratorTarget(
            props.TargetNamespace,
            props.Accessiblity,
            props.TargetClass,
            props.Location,
            props.Injections,
            new CacheableList<(string name, string value)>(builder.ToImmutableArray())
        );
    }

    private void EmitSources(SourceProductionContext ctx, JsonGeneratorTarget target)
    {
        if (target.Members.Count == 0)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(Diagnostics.NoMembersFound, target.Location, target.TargetClass));
            return;
        }

        bool validInjections = true;
        foreach (DataInjectProperties injection in target.Injections)
        {
            if (target.Injections.Count > 1)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diagnostics.MultipleDataInjectParameters, injection.Location, target.TargetClass));
                validInjections = false;
            }
            if (!injection.IsStringParameter)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diagnostics.DataInjectOnNonStringParameter, injection.Location));
                validInjections = false;
            }
        }

        StringBuilder source = new($$"""
            namespace {{target.TargetNamespace}}
            {
                {{target.Accessibility}} static partial class {{target.TargetClass}}
                {

            """);

        if (validInjections && target.Injections.Count == 1)
        {
            DataInjectProperties injection = target.Injections[0];
            StringBuilder parameterDeclarationsBuilder = new();
            StringBuilder parameterCallsFormatBuilder = new();

            for (int i = 0; i < injection.CopiedParameters.Count; i++)
            {
                (string paramType, string paramName) = injection.CopiedParameters[i];
                if (i != 0)
                {
                    parameterDeclarationsBuilder.Append(", ");
                    parameterCallsFormatBuilder.Append(", ");
                }
                if (i == injection.InjectedParameterIndex)
                {
                    parameterCallsFormatBuilder.Append("{0}, ");
                }
                parameterDeclarationsBuilder.Append($"{paramType} {paramName}");
                parameterCallsFormatBuilder.Append(paramName);
            }

            if (injection.InjectedParameterIndex == injection.CopiedParameters.Count)
            {
                if (injection.InjectedParameterIndex > 0)
                {
                    parameterCallsFormatBuilder.Append(", ");
                }
                parameterCallsFormatBuilder.Append("{0}");
            }

            string parameterDeclarations = parameterDeclarationsBuilder.ToString();
            string parameterCallsFormat = parameterCallsFormatBuilder.ToString();

            foreach ((string name, string value) in target.Members)
            {
                SyntaxToken tok = SyntaxFactory.Literal(value);
                string callArgs = string.Format(parameterCallsFormat, tok);
                source.AppendLine($$"""
                        public static {{injection.ReturnType}} Get{{name}}({{parameterDeclarations}}) => {{injection.MethodName}}({{callArgs}});
                """);
            }
        }
        else
        {
            foreach ((string name, string value) in target.Members)
            {
                SyntaxToken tok = SyntaxFactory.Literal(value);
                source.AppendLine($$"""
                        public const string {{name}} = {{tok}};
                """);
            }
        }

        source.AppendLine("""
                }
            }
            """);
        ctx.AddSource(target.TargetClass + ".g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }
}
