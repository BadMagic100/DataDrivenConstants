using DotNet.Globbing;
using Microsoft.CodeAnalysis;

namespace DataDrivenConstants.Util;

internal static class AdditionalTextExtensions
{
    public static bool GlobMatches(this AdditionalText text, string pattern)
    {
        string normalizedPath = text.Path.Replace('\\', '/');
        Glob glob = Glob.Parse(pattern);
        return glob.IsMatch(normalizedPath);
    }
}
