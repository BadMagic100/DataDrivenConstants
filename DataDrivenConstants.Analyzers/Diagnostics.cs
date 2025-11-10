using Microsoft.CodeAnalysis;

namespace DataDrivenConstants;
public static class Diagnostics
{
    public static readonly DiagnosticDescriptor NoMembersFound = new(
        id: "DATACONST001",
        title: "No members found",
        messageFormat: "No constants found for the class {0}. Double check your marker attribute configuration and additional file paths.",
        category: "DataDrivenConstants",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}
