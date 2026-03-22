using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace DataDrivenConstants.Generators;

[Generator(LanguageNames.CSharp)]
public class NameGenerationStyleAttributeGenerator : IIncrementalGenerator
{
    public const string AttributeFullName = "DataDrivenConstants.Marker.NameGenerationStyleAttribute";

    public const string AttributeFileName = "NameGenerationStyleAttribute.g.cs";
    public const string AttributeSource = /*lang=c#-test*/ """
        #nullable enable annotations

        using System;
        
        namespace DataDrivenConstants.Marker
        {
            internal enum NameStyle
            {
                PascalCase,
                CamelCase,
                SnakeCase,
                UpperSnakeCase
            }

            [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            internal sealed class NameGenerationStyleAttribute : Attribute
            {
                public NameStyle NameStyle { get; }

                /// <summary>
                /// Initializes a NameGenerationStyle attribute
                /// </summary>
                /// <param name="nameStyle">The naming style to be used when generating members</param>
                public NameGenerationStyleAttribute(NameStyle nameStyle)
                {
                    this.NameStyle = nameStyle;
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
