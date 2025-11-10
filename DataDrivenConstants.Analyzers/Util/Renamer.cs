using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DataDrivenConstants.Util;

internal static class Renamer
{
    // adapted from https://github.com/dotnet/templating/blob/b0b1283f8c96be35f1b65d4b0c1ec0534d86fc2f/src/Microsoft.TemplateEngine.Orchestrator.RunnableProjects/ValueForms/DefaultSafeNameValueFormFactory.cs#L15-L21
    // under MIT license. Copyright .NET Foundation

    private const string underscore = "_";
    // matches either of the following:
    //   - the zero-length match between a . and a number, or the start of string and a number
    //   - any block of one or more non-word characters, possibly surrounded by up to 1 underscore on each side.
    //     the idea for this behavior with the underscores is that Foo_&_Bar should probably be rendered as Foo_Bar,
    //     but in general we don't want to overeagerly eat underscores
    private static readonly Regex replacer = new(@"(((?<=\.)|^)(?=\d)|_?\W+_?)");

    public static string GetSafeName(string value, IEnumerable<ReplacementRule> customReplacements)
    {
        string workingValue = value.Trim();
        workingValue = workingValue.ApplyReplacements(customReplacements);
        workingValue = replacer.Replace(workingValue, underscore);
        // trim any trailing underscores for convenience
        workingValue = workingValue.TrimEnd('_');
        return workingValue;
    }
}
