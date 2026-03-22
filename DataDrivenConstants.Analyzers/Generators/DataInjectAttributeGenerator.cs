using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace DataDrivenConstants.Generators;

[Generator(LanguageNames.CSharp)]
public class DataInjectAttributeGenerator : IIncrementalGenerator
{
    public const string AttributeFullName = "DataDrivenConstants.Marker.DataInjectAttribute";

    public const string AttributeFileName = "DataInjectAttribute.g.cs";
    public const string AttributeSource = /*lang=c#-test*/ """
        #nullable enable annotations

        using System;
        
        namespace DataDrivenConstants.Marker
        {
            [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
            internal sealed class DataInjectAttribute : Attribute 
            {
                public string Prefix { get; set; } = "Get";
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
