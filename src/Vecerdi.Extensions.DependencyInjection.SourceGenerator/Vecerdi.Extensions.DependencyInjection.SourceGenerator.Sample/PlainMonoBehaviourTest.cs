using UnityEngine;

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

/// <summary>
/// Test class that derives from MonoBehaviour without any injectable properties.
/// This should NOT generate an injector to avoid bloating the assembly.
/// </summary>
public sealed class PlainMonoBehaviourTest : MonoBehaviour {
    // No injectable properties - should not generate an injector
}
