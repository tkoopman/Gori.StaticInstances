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
    public void GeneratorTest(ISimpleTestData data)
    {
        output.WriteLine("--- SOURCE CODE ---");
        output.WriteLine(data.Input);

        string generated = TestHelpers.RunGenerator(data.Input);

        output.WriteLine("");
        output.WriteLine("--- GENERATED CODE ---");
        output.WriteLine(generated);

        data.Assert(generated);
    }

    // -------------------------------------------------------------------------
    // StaticInstance attribute
    // -------------------------------------------------------------------------

    [Fact]
    public void StaticInstance_SimpleArgs_GeneratesNamedProperty()
    {
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        [Singleton]
        public partial class MyService
        {
            private MyService () { }

            [StaticInstance("localhost", 5432, Name = "Dev")]
            private MyService (string host, int port) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("public static MyService Dev { get; } = new MyService(\"localhost\", 5432);", generated);
        Assert.Contains("public static MyService Instance { get; } = new MyService();", generated);
    }

    [Fact]
    public void StaticInstance_MultipleOnSameConstructor_GeneratesBothProperties()
    {
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        [Singleton]
        public partial class MyService
        {
            private MyService () { }

            [StaticInstance("localhost", 5432, Name = "Dev")]
            [StaticInstance("staging-db", 5432, Name = "Staging")]
            private MyService (string host, int port) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("public static MyService Dev { get; } = new MyService(\"localhost\", 5432);", generated);
        Assert.Contains("public static MyService Staging { get; } = new MyService(\"staging-db\", 5432);", generated);
    }

    [Fact]
    public void StaticInstance_InternalFlag_GeneratesInternalProperty()
    {
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        [Singleton]
        public partial class MyService
        {
            private MyService () { }

            [StaticInstance(Name = "Dev", Internal = true)]
            private MyService (string host) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("internal static MyService Dev { get; } = new MyService(", generated);
    }

    [Fact]
    public void StaticInstance_NoArgs_GeneratesNewCall()
    {
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        [Singleton]
        public partial class MyService
        {
            private MyService () { }

            [StaticInstance(Name = "Alternate")]
            private MyService (string host) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("public static MyService Alternate { get; } = new MyService();", generated);
    }

    [Fact]
    public void StaticInstance_DuplicateNameSilentlySkipped_InGenerator()
    {
        // Duplicate names are the analyzer's job; the generator silently skips the second one
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        [Singleton]
        public partial class MyService
        {
            private MyService () { }

            [StaticInstance(Name = "Dev")]
            [StaticInstance(Name = "Dev")]
            private MyService (string host) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        // Only one "Dev" property should appear
        int count = 0;
        int idx = 0;
        while ((idx = generated.IndexOf("static MyService Dev", idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx++;
        }

        Assert.Equal(1, count);
    }

    [Fact]
    public void StaticInstance_NameCollidesWithSingleton_SkippedInGenerator()
    {
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        [Singleton]
        public partial class MyService
        {
            private MyService () { }

            [StaticInstance(Name = "Instance")]
            private MyService (string host) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        // The duplicate "Instance" StaticInstance should be dropped; only the Singleton one survives
        int count = 0;
        int idx = 0;
        while ((idx = generated.IndexOf("static MyService Instance", idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx++;
        }

        Assert.Equal(1, count);
    }

    // -------------------------------------------------------------------------
    // StaticInstance without Singleton
    // -------------------------------------------------------------------------

    [Fact]
    public void StaticInstanceOnly_NoSingleton_GeneratesNamedProperties()
    {
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        public partial class MyService
        {
            [StaticInstance("localhost", 5432, Name = "Dev")]
            [StaticInstance("staging-host", 5432, Name = "Staging")]
            private MyService (string host, int port) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("public static MyService Dev { get; } = new MyService(\"localhost\", 5432);", generated);
        Assert.Contains("public static MyService Staging { get; } = new MyService(\"staging-host\", 5432);", generated);
        Assert.DoesNotContain("Instance", generated);
    }

    [Fact]
    public void StaticInstanceOnly_MultipleConstructors_GeneratesAllProperties()
    {
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        public partial class MyService
        {
            [StaticInstance(Name = "Dev")]
            private MyService (string host) { }

            [StaticInstance(Name = "Prod")]
            private MyService (string host, int port) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("public static MyService Dev { get; } = new MyService();", generated);
        Assert.Contains("public static MyService Prod { get; } = new MyService();", generated);
    }

    [Fact]
    public void StaticInstanceOnly_Constructors_DefaultArg_GeneratesAllProperties()
    {
        string source = """
        using Gori.StaticInstances;
        public partial class MyService
        {
            [StaticInstance([default], Name = "Default")]
            private MyService (int value) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("public static MyService Default { get; } = new MyService((int)default);", generated);
    }

    [Fact]
    public void StaticInstanceOnly_MultipleSimilarConstructors_GeneratesAllProperties()
    {
        string source = """
        using Gori.StaticInstances;
        public partial class MyService
        {
            [StaticInstance(1.0f, Name = "Default1a")]
            [StaticInstance(1, Name = "Default1b")]
            private MyService (int value) { }
        
            [StaticInstance(1, Name = "Default2")]
            private MyService(float value) { }
        
            [StaticInstance(1, Name = "Default3")]
            private MyService(string value) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("public static MyService Default1a { get; } = new MyService(1);", generated);
        Assert.Contains("public static MyService Default1b { get; } = new MyService(1);", generated);
        Assert.Contains("public static MyService Default2 { get; } = new MyService(1f);", generated);
        Assert.Contains("public static MyService Default3 { get; } = new MyService(\"1\");", generated);
    }

    [Fact]
    public void StaticInstanceOnly_DoesNotGeneratePrivateConstructor()
    {
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        public partial class MyService
        {
            [StaticInstance(Name = "Dev")]
            private MyService (string host) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.DoesNotContain("private MyService ()", generated);
    }

    [Fact]
    public void StaticInstance_MultilineStringArg_GeneratesEscapedStringLiteral()
    {
        // Regression: raw/multiline string args must be emitted as escaped regular string literals
        // so the generated file remains valid single-line C#.
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        public partial class MyService
        {
            [StaticInstance("line1\nline2", Name = "Multi")]
            private MyService (string value) { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        // The newline in the arg must be escaped as \n, not emitted literally
        Assert.Contains(@"""line1\nline2""", generated);
        Assert.DoesNotContain("line1\nline2", generated);
    }

    [Fact]
    public void StaticInstance_NullParamsArray_GeneratesNoArgs()
    {
        // A null params array ([StaticInstance(Name = "X", null)]) is treated as 0 supplied args.
        // The generator emits new() with no arguments; GSSI007 will flag this separately.
        string source = """
        using Gori.StaticInstances;
        namespace MyApp;
        public partial class MyService
        {
            [StaticInstance(null, Name = "Null")]
            private MyService () { }
        }
        """;

        string generated = TestHelpers.RunGenerator(source);
        output.WriteLine(generated);

        Assert.Contains("new MyService()", generated);
        Assert.DoesNotContain("null)", generated);
    }
}
