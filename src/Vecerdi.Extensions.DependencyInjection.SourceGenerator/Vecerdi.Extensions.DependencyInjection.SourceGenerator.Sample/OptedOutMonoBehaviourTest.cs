using UnityEngine;

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

/// <summary>
/// Test class that derives from MonoBehaviour but opts out of code generation
/// using the ExcludeFromInjectionGeneration attribute.
/// </summary>
[ExcludeFromInjectionGeneration]
public sealed class OptedOutMonoBehaviourTest : MonoBehaviour {
    [Inject] internal ExampleService ExampleService { get; set; } = null!;
}
