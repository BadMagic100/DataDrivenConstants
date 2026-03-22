using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DataDrivenConstants.Util;

internal static class Renamer
{
    public static string GenerateName(string value, IEnumerable<ReplacementRule> replacements, NameStyle style, string? prefix = null)
    {
        string workingValue = value.Trim();
        workingValue = workingValue.ApplyReplacements(replacements);

        if (style == NameStyle.PascalCase)
        {
            List<string> parts = SeparateSymbolWords(workingValue, prefix);
            for (int i = 0; i < parts.Count; i++)
            {
                // this still preserves upper acronyms
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i][1..];
            }
            workingValue = string.Join("", parts);
        }
        else if (style == NameStyle.CamelCase)
        {
            List<string> parts = SeparateSymbolWords(workingValue, prefix);
            // the first part may be an uppercase acronym, think "HTML" "Parser" so we should lowercase the whole thing for consistency.
            // everything else is the same as pascal case
            parts[0] = parts[0].ToLowerInvariant();
            for (int i = 1; i < parts.Count; i++)
            {
                // this still preserves upper acronyms
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i][1..];
            }
            workingValue = string.Join("", parts);
        }
        else if (style == NameStyle.SnakeCase)
        {
            // snake cases are much easier stylistically
            List<string> parts = SeparateSymbolWords(workingValue, prefix);
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i] = parts[i].ToLowerInvariant();
            }
            workingValue = string.Join("_", parts);
        }
        else if (style == NameStyle.UpperSnakeCase)
        {
            List<string> parts = SeparateSymbolWords(workingValue, prefix);
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i] = parts[i].ToUpperInvariant();
            }
            workingValue = string.Join("_", parts);
        }

        workingValue = GetSafeName(workingValue).TrimEnd('_');
        // this is a slightly strange legacy behavior which may be changed in the far future if another break is needed for some reason
        if (style == NameStyle.MinimalTransform && !string.IsNullOrEmpty(prefix))
        {
            workingValue = prefix + workingValue;
        }
        return workingValue;
    }

    // GetSafeName is adapted from https://github.com/dotnet/templating/blob/b0b1283f8c96be35f1b65d4b0c1ec0534d86fc2f/src/Microsoft.TemplateEngine.Orchestrator.RunnableProjects/ValueForms/DefaultSafeNameValueFormFactory.cs#L15-L21
    // under MIT license. Copyright .NET Foundation

    private const string underscore = "_";
    // matches either of the following:
    //   - the zero-length match between a . and a number, or the start of string and a number
    //   - any block of one or more non-word characters, possibly surrounded by up to 1 underscore on each side.
    //     the idea for this behavior with the underscores is that Foo_&_Bar should probably be rendered as Foo_Bar,
    //     but in general we don't want to overeagerly eat underscores
    private static readonly Regex replacer = new(@"(((?<=\.)|^)(?=\d)|_?\W+_?)");

    private static string GetSafeName(string value)
    {
        return replacer.Replace(value, underscore);
    }

    private enum ParseState
    {
        Start,
        Upper,
        Lower
    }

    public static List<string> SeparateSymbolWords(string text, string? prefix = null)
    {
        List<string> result = [];
        if (string.IsNullOrEmpty(text))
        {
            return result;
        }

        if (!string.IsNullOrEmpty(prefix))
        {
            result.Add(prefix!);
        }
        StringBuilder sb = new();
        ParseState state = ParseState.Start;

        for (int i = 0; i < text.Length; i++)
        {
            // strategy: skip all non-alphanumeric characters.
            // the ways to start a word are:
            // * non-alpha character into an alpha character (separated-case like snake or kebab)
            // * a lowercase character into an uppercase character (pascal/camel case)
            // * an uppercase character that follows consecutive uppercase and is itself followed by a
            //   lowercase character (i.e. the start of a new word after an acronym, like "P" in "HTMLParser")
            if (!char.IsLetterOrDigit(text[i]))
            {
                if (state != ParseState.Start)
                {
                    // end of a word, push it
                    result.Add(sb.ToString());
                    sb.Clear();
                    state = ParseState.Start;
                }
            }
            else
            {
                if (char.IsUpper(text[i]))
                {
                    if (state == ParseState.Lower)
                    {
                        // started new word
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (state == ParseState.Upper && i + 1 < text.Length && char.IsLower(text[i + 1]))
                    {
                        // started new word after acronym
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    state = ParseState.Upper;
                    sb.Append(text[i]);
                }
                else
                {
                    state = ParseState.Lower;
                    sb.Append(text[i]);
                }
            }
        }

        if (state != ParseState.Start)
        {
            result.Add(sb.ToString());
        }
        return result;
    }
}
