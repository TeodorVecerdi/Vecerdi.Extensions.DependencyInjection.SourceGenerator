using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator;

[Generator]
public class DependencyInjectionSourceGenerator : IIncrementalGenerator {
    private const string InfrastructureNamespace = "Vecerdi.Extensions.DependencyInjection.Infrastructure";
    private const string DefaultNamespace = "Vecerdi.Extensions.DependencyInjection";
    private const string ContextBaseClassName = "TypeInjectorResolverContext";
    private const string BaseMonoBehaviourName = "BaseMonoBehaviour";
    private const string InjectAttributeName = "InjectAttribute";
    private const string InjectFromKeyedServicesAttributeName = "InjectFromKeyedServicesAttribute";
    private const string ExcludeFromInjectionGenerationAttributeName = "ExcludeFromInjectionGenerationAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // Filter for partial classes that inherit from TypeInjectorResolverContext
        var contextClassProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax cds && cds.Modifiers.Any(SyntaxKind.PartialKeyword),
                transform: static (ctx, _) => GetContextClassForGeneration(ctx))
            .Where(static t => t.IsContextClass);

        // Collect all context classes and combine with compilation
        var combinedProvider = context.CompilationProvider.Combine(contextClassProvider.Collect());

        // Generate code for each discovered context
        context.RegisterSourceOutput(combinedProvider, static (spc, source) => GenerateCode(spc, source.Left, source.Right));
    }

    private static (ClassDeclarationSyntax ClassSyntax, bool IsContextClass) GetContextClassForGeneration(GeneratorSyntaxContext context) {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classSyntax);

        if (symbol == null) return (classSyntax, false);

        // Check if it inherits from TypeInjectorResolverContext (direct or indirect)
        var baseType = symbol.BaseType;
        while (baseType != null) {
            if (baseType.Name == ContextBaseClassName && baseType.ContainingNamespace.ToDisplayString() == InfrastructureNamespace)
                return (classSyntax, true);
            baseType = baseType.BaseType;
        }

        return (classSyntax, false);
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation, ImmutableArray<(ClassDeclarationSyntax ClassSyntax, bool IsContextClass)> contextClassTuples) {
        if (contextClassTuples.IsEmpty)
            return;

        // Collect all eligible types in the compilation
        var eligibleTypes = new List<(INamedTypeSymbol TypeSymbol, List<(IPropertySymbol Property, object? Key, bool IsRequired)> Properties)>();

        foreach (var syntaxTree in compilation.SyntaxTrees) {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>()) {
                var typeSymbol = semanticModel.GetDeclaredSymbol(classDecl);
                if (typeSymbol == null || typeSymbol.IsAbstract || typeSymbol.TypeKind != TypeKind.Class)
                    continue;

                // Check exclusion
                if (typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == ExcludeFromInjectionGenerationAttributeName))
                    continue;

                // Check inheritance from BaseMonoBehaviour
                if (!InheritsFrom(typeSymbol, BaseMonoBehaviourName, DefaultNamespace)) continue;

                // Check for at least one valid [Inject*] property
                var injectProperties = GetInjectProperties(typeSymbol, semanticModel, classDecl, context);
                eligibleTypes.Add((typeSymbol, injectProperties));
            }
        }

        // Sort eligible types for consistent output
        eligibleTypes.Sort((a, b) => string.CompareOrdinal(a.TypeSymbol.ToDisplayString(), b.TypeSymbol.ToDisplayString()));

        // For each context class, generate code
        foreach (var (classSyntax, _) in contextClassTuples) {
            var contextSymbol = compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);
            if (contextSymbol == null) continue;

            if (contextSymbol.IsGenericType || contextSymbol.TypeParameters.Length > 0) {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.UnsupportedGenericContext, classSyntax.GetLocation(), contextSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            if (contextClassTuples.Length > 1) {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.MultipleContexts, classSyntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }

            var typeNames = new Dictionary<string, string>();
            var usedTypeNames = new HashSet<string>();
            var namespaceName = contextSymbol.ContainingNamespace.IsGlobalNamespace ? null : contextSymbol.ContainingNamespace.ToDisplayString();
            var contextClassName = contextSymbol.Name;

            // Extract modifiers from user's syntax (e.g., public, internal)
            var modifiers = string.Join(" ", classSyntax.Modifiers.Select(m => m.Text));

            // Build the generated code
            var codeBuilder = new StringBuilder();
            codeBuilder.AppendLine("""
                                   // <auto-generated/>
                                   #nullable enable
                                   using System;
                                   using Microsoft.Extensions.DependencyInjection;
                                   using Vecerdi.Extensions.DependencyInjection;
                                   using Vecerdi.Extensions.DependencyInjection.Infrastructure;
                                   """);

            if (!string.IsNullOrEmpty(namespaceName)) {
                codeBuilder.AppendLine($$"""
                                         namespace {{namespaceName}}
                                         {
                                         """);
            }

            codeBuilder.AppendLine($$"""
                                         {{modifiers}} class {{contextClassName}}
                                         {
                                             public override ITypeInjector? GetTypeInjector(Type type)
                                             {
                                     """);

            if (eligibleTypes.Count == 0) {
                codeBuilder.AppendLine($"            return null; // No eligible types found for context '{contextClassName}'.");
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.NoEligibleTypes, classSyntax.GetLocation(), contextClassName);
                context.ReportDiagnostic(diagnostic);
            } else {
                codeBuilder.AppendLine("""
                                                   return type.FullName switch
                                                   {
                                       """);
                foreach (var (type, _) in eligibleTypes) {
                    var typeName = GetUniqueTypeName(type, typeNames, usedTypeNames);
                    var typeFullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
                    codeBuilder.AppendLine($"""                "{typeFullName}" => {typeName}Injector.Instance,""");
                }

                codeBuilder.AppendLine("                _ => null");
                codeBuilder.AppendLine("            };");
            }

            codeBuilder.AppendLine("        }");

            // Generate nested injector classes
            foreach (var (type, properties) in eligibleTypes) {
                var typeName = GetUniqueTypeName(type, typeNames, usedTypeNames);
                var injectorClassName = $"{typeName}Injector";
                var typeFullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                codeBuilder.AppendLine(
                    $$"""

                              private sealed class {{injectorClassName}} : ITypeInjector
                              {
                                  public static readonly {{injectorClassName}} Instance = new();
                                  public void Inject(IServiceProvider serviceProvider, object instance)
                                  {
                      """);

                if (properties.Count == 0) {
                    codeBuilder.AppendLine($"                // No injectable properties found for type '{typeName}'.");
                } else {
                    codeBuilder.AppendLine($"                var typedInstance = ({typeFullName})instance;");

                    foreach (var (prop, key, isRequired) in properties) {
                        var propType = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        var propName = prop.Name;

                        var keyLiteral = FormatAsCSharpLiteral(key);

                        string getService;
                        if (key == null) {
                            getService = $"serviceProvider.GetService(typeof({propType}))";
                        } else {
                            getService = $"(serviceProvider as IKeyedServiceProvider)?.GetKeyedService(typeof({propType}), {keyLiteral})";
                        }

                        codeBuilder.AppendLine($"                var {propName}Service = {getService};");
                        if (isRequired) {
                            codeBuilder.AppendLine($"""
                                                                    if ({propName}Service == null)
                                                                        throw new InvalidOperationException("Required service {propType} is not registered.");
                                                    """);
                            codeBuilder.AppendLine($"                typedInstance.{propName} = ({propType}){propName}Service;");
                        } else {
                            codeBuilder.AppendLine($"""
                                                                    if ({propName}Service != null)
                                                                        typedInstance.{propName} = ({propType}){propName}Service;
                                                    """);
                        }
                    }
                }

                codeBuilder.AppendLine("""
                                                   }
                                               }
                                       """);
            }

            codeBuilder.AppendLine("    }");

            if (!string.IsNullOrEmpty(namespaceName)) {
                codeBuilder.AppendLine("}");
            }

            // Add to context
            var generatedCode = codeBuilder.ToString();
            context.AddSource($"{contextClassName}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
        }
    }

    private static string GetUniqueTypeName(INamedTypeSymbol typeSymbol, Dictionary<string, string> typeMap, HashSet<string> usedTypeNames) {
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (typeMap.TryGetValue(fullTypeName, out var typeName)) {
            return typeName;
        }

        typeName = typeSymbol.Name;
        var i = 1;
        while (usedTypeNames.Contains(typeName)) {
            typeName = $"{typeName}{i++}";
        }

        usedTypeNames.Add(typeName);
        typeMap[fullTypeName] = typeName;
        return typeName;
    }

    // Helper: Format object? as C# literal for codegen
    private static string FormatAsCSharpLiteral(object? value) {
        if (value == null) return "null";
        if (value is string s) return $"\"{s.Replace("\"", "\\\"")}\"";
        if (value is bool b) return b ? "true" : "false";
        if (value is int or uint or long or ulong or short or ushort or byte or sbyte or float or double or decimal) return value.ToString()!;
        if (value is char c) return $"'{c}'";

        return "null /* Unsupported key type */";
    }

    // Helper: Check inheritance
    private static bool InheritsFrom(INamedTypeSymbol? symbol, string baseName, string baseNamespace) {
        var baseType = symbol?.BaseType;
        while (baseType != null) {
            if (baseType.Name == baseName && baseType.ContainingNamespace.ToDisplayString() == baseNamespace)
                return true;
            baseType = baseType.BaseType;
        }

        return false;
    }

    // Helper: Get properties with exactly one [Inject] or [InjectFromKeyedServices], extracting params
    private static List<(IPropertySymbol Property, object? Key, bool IsRequired)> GetInjectProperties(INamedTypeSymbol typeSymbol, SemanticModel semanticModel, ClassDeclarationSyntax classDecl, SourceProductionContext context) {
        var results = new List<(IPropertySymbol, object?, bool)>();

        foreach (var prop in typeSymbol.GetMembers().OfType<IPropertySymbol>()) {
            if (prop.IsStatic) continue;

            var attributes = prop.GetAttributes()
                .Where(a => a.AttributeClass != null && a.AttributeClass.Name is InjectAttributeName or InjectFromKeyedServicesAttributeName && a.AttributeClass.ContainingNamespace.ToDisplayString() == DefaultNamespace)
                .ToList();
            if (attributes.Count == 0) {
                continue;
            }

            if (attributes.Count > 1) {
                // Find property syntax for accurate location
                var propSyntax = classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                    .FirstOrDefault(p => semanticModel.GetDeclaredSymbol(p)?.Name == prop.Name);
                var location = propSyntax?.GetLocation() ?? classDecl.GetLocation();

                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.MultipleInjectAttributes, location, prop.Name, typeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            // Check setter accessibility (must be public, internal, or protected internal for direct access in generated code)
            var setter = prop.SetMethod;
            if (setter == null || setter.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Internal and not Accessibility.ProtectedOrInternal) {
                var propSyntax = classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                    .FirstOrDefault(p => semanticModel.GetDeclaredSymbol(p)?.Name == prop.Name);
                var location = propSyntax?.GetLocation() ?? classDecl.GetLocation();

                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.InaccessibleProperty, location, prop.Name, typeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            if (setter.IsInitOnly) {
                var propSyntax = classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                    .FirstOrDefault(p => semanticModel.GetDeclaredSymbol(p)?.Name == prop.Name);
                var location = propSyntax?.GetLocation() ?? classDecl.GetLocation();
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.InitOnlyProperty, location, prop.Name, typeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            // Extract params from the single attribute
            var attr = attributes[0];
            object? key = null;
            var isRequired = true;

            if (attr.AttributeClass!.Name == InjectAttributeName) {
                // Non-keyed: Only isRequired (arg 0)
                if (attr.ConstructorArguments.Length > 0)
                    isRequired = (bool)attr.ConstructorArguments[0].Value!;
            } else if (attr.AttributeClass.Name == InjectFromKeyedServicesAttributeName) {
                // Keyed: serviceKey (arg 0), isRequired (arg 1)
                if (attr.ConstructorArguments.Length > 0)
                    key = attr.ConstructorArguments[0].Value;
                if (attr.ConstructorArguments.Length > 1)
                    isRequired = (bool)attr.ConstructorArguments[1].Value!;
            }

            results.Add((prop, key, isRequired));
        }

        return results;
    }
}
