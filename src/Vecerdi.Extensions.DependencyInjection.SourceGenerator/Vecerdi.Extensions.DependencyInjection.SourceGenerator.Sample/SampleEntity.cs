namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

public class SampleEntity : BaseMonoBehaviour {
    [Inject] internal ExampleService ExampleService { get; set; } = null!;
    [InjectFromKeyedServices("key")] internal ExampleService ExampleService2 { get; set; } = null!;
}
