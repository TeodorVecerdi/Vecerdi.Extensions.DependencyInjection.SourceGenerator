namespace Vecerdi.Extensions.DependencyInjection.SourceGenerator.Sample;

// This code will not compile until you build the project with the Source Generators

public class Examples {
    // Execute generated method Report
    public static IEnumerable<string> CreateEntityReport(SampleEntity entity) {
        return entity.Report();
    }
}
