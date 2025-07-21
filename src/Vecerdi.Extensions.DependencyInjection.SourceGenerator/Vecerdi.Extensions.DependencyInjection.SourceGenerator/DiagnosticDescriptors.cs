using Microsoft.CodeAnalysis;

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator;

public static class DiagnosticDescriptors {
    public static readonly DiagnosticDescriptor PropertyHasNoSetter = new(
        "VDI0001",
        "Property has no setter",
        "Type '{0}' has [Inject] properties without setters; skipping generation",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor UnsupportedGenericContext = new(
        "VDI0002",
        "Unsupported generic context class",
        "Generic context class '{}' is not supported; skipping",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor MultipleContexts = new(
        "VDI0003",
        "Multiple contexts found",
        "Multiple injection contexts found; ensure they don't conflict",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor NoEligibleTypes = new(
        "VDI0004",
        "No eligible types found",
        "No eligible types found for context '{0}'",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor InaccessibleProperty = new(
        "VDI0005",
        "Inaccessible property",
        "[Inject] property '{0}' in type '{1}' has inaccessible setter (must be public, internal, or protected internal); skipping property",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor MultipleInjectAttributes = new(
        "VDI0006",
        "Multiple inject attributes",
        "Property '{0}' in type '{1}' has multiple [Inject] attributes; only one is allowed",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Error,
        true
    );
}
