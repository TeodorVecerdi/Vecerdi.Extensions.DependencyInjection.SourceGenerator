using System;
using Vecerdi.Extensions.DependencyInjection;

namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

public sealed class ServiceProviderTestService : BaseMonoBehaviour {
    // Test IServiceProvider injection - should be assigned directly from serviceProvider parameter
    [Inject] public IServiceProvider? ServiceProvider { get; set; }

    // Test that regular services still work alongside IServiceProvider
    [Inject] public ExampleService? ExampleService { get; set; }

    // Test keyed IServiceProvider (should still assign directly, ignoring the key)
#pragma warning disable VDI0007
    [InjectFromKeyedServices("test")] public IServiceProvider? KeyedServiceProvider { get; set; }
#pragma warning restore VDI0007

    // Test optional IServiceProvider (should still assign directly, ignoring isRequired)
    [Inject(isRequired: false)] public IServiceProvider? OptionalServiceProvider { get; set; }
}
