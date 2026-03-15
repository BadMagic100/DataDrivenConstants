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


    public static readonly DiagnosticDescriptor AttributeOnNonStaticPartialClass = new(
        id: "DATACONST002",
        title: "DataDrivenConstants attribute applied to non-static or non-partial class",
        messageFormat: "DataDrivenConstants marker attributes may only be applied to static partial classes, but target class {0} is not",
        category: "DataDrivenConstants",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MultipleDataInjectParameters = new(
        id: "DATACONST003",
        title: "DataInject attribute applied in multiple locations",
        messageFormat: "DataInject attributes may only be applied to a single parameter of a single method, but target class {0} contains multiple",
        category: "DataDrivenConstants",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor DataInjectOnNonStringParameter = new(
        id: "DATACONST004",
        title: "DataInject attribute applied to non-string parameter",
        messageFormat: "DataInject attributes may only be applied to string parameters",
        category: "DataDrivenConstants",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
