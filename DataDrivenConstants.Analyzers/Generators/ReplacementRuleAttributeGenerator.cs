using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace DataDrivenConstants.Generators;

[Generator(LanguageNames.CSharp)]
public class ReplacementRuleAttributeGenerator : IIncrementalGenerator
{
    public const string AttributeFullName = "DataDrivenConstants.Marker.ReplacementRuleAttribute";

    public const string AttributeFileName = "ReplacementRuleAttribute.g.cs";
    public const string AttributeSource = /*lang=c#-test*/ """
        #nullable enable annotations

        using System;
        
        namespace DataDrivenConstants.Marker
        {
            internal enum ReplacementKind
            {
                Normal,
                Regex
            }

            [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
            internal sealed class ReplacementRuleAttribute : Attribute
            {
                public string OldString { get; }
                public string NewString { get; }
                public ReplacementKind ReplacementKind { get; }

                /// <summary>
                /// Initializes a ReplacementRule attribute
                /// </summary>
                /// <param name="oldString">The old string to be replaced</param>
                /// <param name="newString">The new string to replace the old string with</param>
                /// <param name="replacementKind">What method to use to perform the replacement</param>
                public ReplacementRuleAttribute(
                    string oldString, 
                    string newString, 
                    ReplacementKind replacementKind = ReplacementKind.Normal
                )
                {
                    OldString = oldString;
                    NewString = newString;
                    ReplacementKind = replacementKind;
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
