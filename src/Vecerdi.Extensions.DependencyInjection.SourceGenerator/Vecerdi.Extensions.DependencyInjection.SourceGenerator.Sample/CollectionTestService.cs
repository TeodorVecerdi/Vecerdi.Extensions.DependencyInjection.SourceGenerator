namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

public sealed class CollectionTestService : BaseMonoBehaviour {
    // Test IEnumerable<T> - should use GetServices() directly
    [Inject] public IEnumerable<ExampleService>? EnumerableServices { get; set; }

    // Test T[] - should use GetServices().ToArray()
    [Inject] public ExampleService[]? ArrayServices { get; set; }

    // Test IReadOnlyCollection<T> - should use GetServices().ToArray()
    [Inject] public IReadOnlyCollection<ExampleService>? ReadOnlyCollectionServices { get; set; }

    // Test IReadOnlyList<T> - should use GetServices().ToArray()
    [Inject] public IReadOnlyList<ExampleService>? ReadOnlyListServices { get; set; }

    // Test List<T> - should use GetServices().ToList()
    [Inject] public List<ExampleService>? ListServices { get; set; }

    // Test IList<T> - should use GetServices().ToList()
    [Inject] public IList<ExampleService>? ListServicesInterface { get; set; }

    // Test ICollection<T> - should use GetServices().ToList()
    [Inject] public ICollection<ExampleService>? CollectionServices { get; set; }

    // Test keyed services with collections
    [InjectFromKeyedServices("test")] public IEnumerable<ExampleService>? KeyedEnumerableServices { get; set; }
    [InjectFromKeyedServices("test")] public ExampleService[]? KeyedArrayServices { get; set; }
    [InjectFromKeyedServices("test")] public List<ExampleService>? KeyedListServices { get; set; }

    // Test regular single service injection (should remain unchanged)
    [Inject] public ExampleService? SingleService { get; set; }
    [InjectFromKeyedServices("single")] public ExampleService? KeyedSingleService { get; set; }
}
