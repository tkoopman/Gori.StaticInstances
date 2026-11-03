using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Generators;
using Gori.StaticInstances.Testing.TestData;

namespace Gori.StaticInstances.Testing;

public class GeneratorTests(ITestOutputHelper output)
{
    // -------------------------------------------------------------------------
    // Source code generation
    // -------------------------------------------------------------------------

    [Theory]
    [ClassData(typeof(Generator_TestData))]
    [ClassData(typeof(CombinedGenerator_TestData))]
    [ClassData(typeof(SingletonGenerator_TestData))]
    [ClassData(typeof(StaticInstanceGenerator_TestData))]
    public async Task SourceGeneratorTest(MockSourceData data)
        => await MockProject.AssertGeneratorTest<StaticInstancesGenerator>(data, output);

    [Theory]
    [ClassData(typeof(AttributesGenerator_TestData))]
    public async Task AttributesGeneratorTest(MockSourceData data)
        => await MockProject.AssertGeneratorTest<AttributesGenerator>(data, output);
}
