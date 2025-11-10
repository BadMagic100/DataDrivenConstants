using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace DataDrivenConstants.Generators;

[Generator(LanguageNames.CSharp)]
public class JsonDataAttributeGenerator : IIncrementalGenerator
{
    public const string AttributeFullName = "DataDrivenConstants.Marker.JsonDataAttribute";

    public const string AttributeFileName = "JsonDataAttribute.g.cs";
    public const string AttributeSource = /*lang=c#-test*/ """
        #nullable enable annotations

        using System;
        
        namespace DataDrivenConstants.Marker
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            internal sealed class JsonDataAttribute : Attribute
            {
                public string ValuePath { get; }
                public string[] FileGlobs { get; }

                public JsonDataAttribute(string valuePath, params string[] fileGlobs)
                {
                    ValuePath = valuePath;
                    FileGlobs = (string[])fileGlobs.Clone();
                }
            }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(AttributeFileName, SourceText.From(AttributeSource, Encoding.UTF8));
        });
    }
}
