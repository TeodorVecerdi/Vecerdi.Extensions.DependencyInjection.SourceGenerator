# Source Generator for Vecerdi.Extensions.DependencyInjection

A C# source generator that replaces reflection-based dependency injection with compile-time generated code for optimal performance in Unity projects. This source generator works in conjunction with [Vecerdi.Extensions.DependencyInjection](https://github.com/TeodorVecerdi/Vecerdi.Extensions.DependencyInjection) to provide zero-reflection service injection.

## Features

- 🚀 **Zero reflection** - Generates concrete injection code at compile time
- ⚡ **High performance** - Eliminates runtime reflection overhead
- 🎯 **Type-safe** - Compile-time validation of injection targets
- 🔍 **Comprehensive diagnostics** - Helpful error messages and warnings
- 🧩 **Seamless integration** - Drop-in replacement for reflection-based injection

## How It Works

The source generator analyzes your code and automatically generates optimized `ITypeInjector` implementations for classes that inherit from `BaseMonoBehaviour` and have properties marked with `[Inject]` or `[InjectFromKeyedServices]` attributes.

Instead of using reflection at runtime, the generator creates concrete code that directly sets your properties with the resolved services.

## Installation

### 1. Build the Source Generator

Clone and build the source generator project:

```bash
git clone https://github.com/TeodorVecerdi/Vecerdi.Extensions.DependencyInjection.SourceGenerator.git
cd Vecerdi.Extensions.DependencyInjection.SourceGenerator/src
dotnet build
```

### 2. Copy the DLL to Your Unity Project

After building, copy the generated DLL from the artifacts folder to your Unity project:

```bash
# Copy from build output
cd .. # go up from the src/ directory
cp artifacts/bin/Vecerdi.Extensions.DependencyInjection.SourceGenerator/debug/Vecerdi.Extensions.DependencyInjection.SourceGenerator.dll YourUnityProject/Assets/Plugins/
```


### 3. Configure the DLL as a Roslyn Analyzer

1. Select the DLL in the Unity Project window
2. In the Inspector, configure the following settings:
    - **Settings for 'Any Platform'**: Uncheck "Any Platform"
    - **Platform Compatibility**: Uncheck all platforms
    - **Asset Labels**: Add label "RoslynAnalyzer"

### 4. Verify Installation

After importing and configuring the DLL, Unity should automatically start using the source generator during compilation. You can verify it's working by:

1. Creating a partial class that inherits from `TypeInjectorResolverContext`
2. Code should compile. If the source generator doesn't work, you'll get a compiler error saying that your `TypeInjectorResolverContext` doesn't implement the required method.

For more detailed information about setting up Roslyn analyzers in Unity, see the [official Unity documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/roslyn-analyzers.html).

## Usage

### 1. Create a Type Injector Resolver Context

Create a partial class that inherits from `TypeInjectorResolverContext`:

```csharp
using Vecerdi.Extensions.DependencyInjection.Infrastructure;

namespace MyProject.DependencyInjection
{
    public partial class GeneratedTypeInjectorResolver : TypeInjectorResolverContext
    {
        // Implementation will be generated automatically
    }
}
```

### 2. Configure the Service Manager

Replace the default reflection-based resolver with your generated one:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void SetupDependencyInjection()
{
    // Use the generated resolver instead of reflection
    ServiceManager.Resolver = new GeneratedTypeInjectorResolver();
    
    // Or use reflection as a fallback (recommended)
    ServiceManager.Resolver = new TypeInjectorResolverCombiner(
        new GeneratedTypeInjectorResolver(),
        new ReflectionTypeInjectorResolver()
    );
    
    // Register your services as usual
    ServiceManager.RegisterServices((services, config) =>
    {
        services.AddSingleton<IGameSettings, GameSettings>();
        services.AddMonoBehaviourSingleton<PlayerController>();
        // ... other services
    });
}
```

### 3. Use Injection Attributes

Mark your properties for injection in classes that inherit from `BaseMonoBehaviour`:

```csharp
namespace MyProject
{
    public class PlayerController : BaseMonoBehaviour
    {
        [Inject] public IGameSettings GameSettings { get; set; }
        [Inject] public ILogger Logger { get; set; }
        [Inject(isRequired: false)] public IOptionalService? OptionalService { get; set; }
        
        // Keyed service injection
        [InjectFromKeyedServices("primary")] public IWeaponController PrimaryWeapon { get; set; }
        [InjectFromKeyedServices("secondary", isRequired: false)] public IWeaponController SecondaryWeapon { get; set; }
        
        void Start()
        {
            // All services are injected automatically with zero reflection!
            Logger.Log($"Player speed: {GameSettings.PlayerSpeed}");
        }
    }
}
```

## Generated Code Example

For the `PlayerController` above, the source generator creates code similar to:

```csharp
// <auto-generated/>
#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Vecerdi.Extensions.DependencyInjection;
using Vecerdi.Extensions.DependencyInjection.Infrastructure;

public partial class GeneratedTypeInjectorResolver
{
    public override ITypeInjector? GetTypeInjector(Type type)
    {
        return type.FullName switch
        {
            "MyProject.PlayerController" => PlayerControllerInjector.Instance,
            _ => null
        };
    }

    private sealed class PlayerControllerInjector : ITypeInjector
    {
        public static readonly PlayerControllerInjector Instance = new();
        
        public void Inject(IServiceProvider serviceProvider, object instance)
        {
            var typedInstance = (MyProject.PlayerController)instance;
            typedInstance.GameSettings = serviceProvider.GetRequiredService<IGameSettings>();
            typedInstance.Logger = serviceProvider.GetRequiredService<ILogger>();
            typedInstance.OptionalService = serviceProvider.GetService<IOptionalService>();
            typedInstance.PrimaryWeapon = serviceProvider.GetRequiredKeyedService<IWeaponController>("primary");
            typedInstance.SecondaryWeapon = serviceProvider.GetKeyedService<IWeaponController>("secondary");
        }
    }
}
```

## Diagnostics and Error Handling

The source generator provides helpful diagnostics:

| Code    | Severity | Description                                  |
|---------|----------|----------------------------------------------|
| VDI0001 | Warning  | Property is init-only and cannot be injected |
| VDI0002 | Warning  | Generic context classes are not supported    |
| VDI0003 | Info     | Multiple injection contexts found            |
| VDI0004 | Info     | No eligible types found for context          |
| VDI0005 | Warning  | Property has inaccessible setter             |
| VDI0006 | Error    | Property has multiple inject attributes      |

## Requirements

- Properties marked for injection must have public, internal, or protected internal setters
- Properties cannot be init-only
- Only one injection attribute per property (`[Inject]` or `[InjectFromKeyedServices]`)
- Target classes must inherit from `BaseMonoBehaviour`

## Excluding Types from Generation

Use the `[ExcludeFromInjectionGeneration]` attribute to exclude specific types:

```csharp
[ExcludeFromInjectionGeneration]
public class SpecialController : BaseMonoBehaviour
{
    [Inject] public IService Service { get; set; }
    // This class will use reflection-based injection
}
```

## Performance Benefits

- **Compile-time code generation** eliminates reflection overhead
- **Optimized property access** uses direct assignment instead of reflection
- **Reduced allocations** from eliminating reflection-based operations

## Supported Service Key Types

The generator supports these key types for keyed services:

- `string`
- `bool`
- Numeric types (`int`, `uint`, `long`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `float`, `double`, `decimal`)
- `char`
- `null`

## Integration with Main Library

This source generator is designed to work seamlessly with [Vecerdi.Extensions.DependencyInjection](https://github.com/TeodorVecerdi/Vecerdi.Extensions.DependencyInjection). Simply replace the default `ReflectionTypeInjectorResolver` with your generated resolver to get automatic performance improvements with no code changes required.

## License

This project is licensed under the MIT license with additional terms regarding AI usage. See the [LICENSE](./LICENSE) file.
