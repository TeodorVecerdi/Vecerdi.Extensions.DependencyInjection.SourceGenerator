using UnityEngine;

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

/// <summary>
/// Test class that derives directly from MonoBehaviour (not BaseMonoBehaviour)
/// to verify the source generator works with any MonoBehaviour-derived type.
/// </summary>
public sealed class DirectMonoBehaviourTest : MonoBehaviour {
    [Inject] internal ExampleService ExampleService { get; set; } = null!;
    [Inject(false)] internal ExampleService? OptionalService { get; set; }
}
