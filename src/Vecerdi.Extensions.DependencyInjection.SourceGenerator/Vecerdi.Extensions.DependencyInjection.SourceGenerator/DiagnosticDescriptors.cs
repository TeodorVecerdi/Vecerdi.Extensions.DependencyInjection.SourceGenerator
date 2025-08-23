using Microsoft.CodeAnalysis;

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator;

public static class DiagnosticDescriptors {
    public static readonly DiagnosticDescriptor InitOnlyProperty = new(
        "VDI0001",
        "Property is init-only",
        "Property '{0}' in type '{1}' has is init-only and cannot be injected",
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
        "Property '{0}' in type '{1}' has inaccessible setter (must be public, internal, or protected internal)",
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

    public static readonly DiagnosticDescriptor KeyedServiceProviderIgnored = new(
        "VDI0007",
        "Service key ignored for IServiceProvider",
        "Property '{0}' in type '{1}' uses [InjectFromKeyedServices] but IServiceProvider injection ignores the service key",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Warning,
        true
    );
}
