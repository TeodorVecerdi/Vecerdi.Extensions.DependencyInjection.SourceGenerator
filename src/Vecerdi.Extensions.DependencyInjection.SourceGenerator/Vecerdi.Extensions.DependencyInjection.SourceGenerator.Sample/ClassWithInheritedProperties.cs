namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

public class BaseClassWithProperties : BaseMonoBehaviour {
    [Inject] protected internal ExampleService ExampleService { get; set; } = null!;
}

public sealed class ClassWithInheritedProperties : BaseClassWithProperties {
    [Inject] internal ExampleService ExampleService2 { get; set; } = null!;
}
