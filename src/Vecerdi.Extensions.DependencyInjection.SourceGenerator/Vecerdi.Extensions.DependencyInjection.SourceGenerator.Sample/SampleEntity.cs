using Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample {
    public sealed class SampleEntity : BaseMonoBehaviour {
        [Inject] internal ExampleService ExampleService { get; set; } = null!;
        [InjectFromKeyedServices("key")] internal ExampleService ExampleService2 { get; set; } = null!;
    }

    public sealed class SampleEntityWithoutServices : BaseMonoBehaviour {
    }
}

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample2 {
    // Same type name as the other type above
    public sealed class SampleEntityWithoutServices : BaseMonoBehaviour {
        [Inject(false)] internal ExampleService? ExampleService { get; set; } = null!;
    }
}
