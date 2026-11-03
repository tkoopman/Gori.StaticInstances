using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Gori.StaticInstances.Testing;

public class GeneratorTypeTests(ITestOutputHelper output)
{
    [Fact]
    public async Task DuplicateNamesAcrossPartialDeclarations_GenerateOneCompilableFile()
    {
        var project = new MockProject(typeof(StaticInstancesGenerator).Assembly, cancellationToken: TestContext.Current.CancellationToken);
        _ = project.AddDocument(
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(2, Name = "Duplicate")]
                private MyService(int value) { }
            }
            """, "Second.cs");

        _ = project.AddDocument(
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance("first", Name = "Duplicate")]
                private MyService(string value) { }
            }
            """, "First.cs");

        GeneratorRunResult result = await GenerateAndCheckCompilation(project);
        GeneratedSourceResult source = Assert.Single(result.GeneratedSources);
        Assert.Equal("MyService.StaticInstances.g.cs", source.HintName);
        Assert.Contains("public static MyService Duplicate { get; } = new MyService(\"first\");", source.SourceText.ToString());
        Assert.DoesNotContain("new MyService(2)", source.SourceText.ToString());
    }

    [Fact]
    public async Task SingletonAndStaticInstances_AllocateDistinctFieldAndLockNames()
    {
        var project = new MockProject(typeof(StaticInstancesGenerator).Assembly, cancellationToken: TestContext.Current.CancellationToken);
        _ = project.AddDocument(
            """
            using Gori.StaticInstances;
            public class Parent
            {
                protected object _foo = new object();
            }

            [Singleton(Name = "Foo", Type = InstanceType.Weak)]
            public partial class MyService : Parent
            {
                [StaticInstance(Name = "foo", Type = InstanceType.Weak)]
                [StaticInstance(Name = "_foo1WeakLock", Type = InstanceType.Field)]
                [StaticInstance(Name = "Foo", Type = InstanceType.Lazy)]
                private MyService() { }
            }
            """);

        GeneratorRunResult result = await GenerateAndCheckCompilation(project);
        GeneratedSourceResult source = Assert.Single(result.GeneratedSources);
        string code = source.SourceText.ToString();
        Assert.Equal("MyService.StaticInstances.g.cs", source.HintName);
        Assert.Contains("global::System.WeakReference<MyService> _foo1 =", code);
        Assert.Contains("global::System.WeakReference<MyService> _foo2 =", code);
        Assert.Contains("object _foo1WeakLock1 =", code);
        Assert.Contains("object _foo2WeakLock =", code);
        Assert.Contains("public static readonly MyService _foo1WeakLock =", code);
        Assert.DoesNotContain("global::System.Lazy", code);
    }

    [Fact]
    public async Task SameTypeNamesInDifferentNamespacesAndGenericArities_GenerateSeparateFiles()
    {
        var project = new MockProject(typeof(StaticInstancesGenerator).Assembly, cancellationToken: TestContext.Current.CancellationToken);
        _ = project.AddDocument(
            """
            using Gori.StaticInstances;
            namespace First
            {
                [Singleton]
                public partial class Service { }

                [Singleton]
                public partial class Service<T> { }
            }

            namespace Second
            {
                [Singleton]
                public partial class Service { }
            }
            """);

        GeneratorRunResult result = await GenerateAndCheckCompilation(project);
        Assert.Equal(
            ["First.Service.StaticInstances.g.cs", "First.Service`1.StaticInstances.g.cs", "Second.Service.StaticInstances.g.cs"],
            [.. result.GeneratedSources.Select(static source => source.HintName).Order(StringComparer.Ordinal)]);
    }

    [Fact]
    public async Task EditingOneType_CachesOtherTypeAndRefreshesInheritedNameConflicts()
    {
        var project = new MockProject(typeof(StaticInstancesGenerator).Assembly, cancellationToken: TestContext.Current.CancellationToken);
        _ = project.AddDocument(
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.Lazy)]
            public partial class Unchanged { }
            """, "Unchanged.cs");

        _ = project.AddDocument(
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.Lazy)]
            public partial class Changed : Parent { }
            """, "Changed.cs");

        _ = project.AddDocument("public class Parent { }", "Parent.cs");

        Compilation compilation = await project.GetCompilationAsync();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new StaticInstancesGenerator().AsSourceGenerator()],
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        GeneratorDriverRunResult initial = driver.GetRunResult();
        output.Write(initial);
        Assert.Null(Assert.Single(initial.Results).Exception);
        Assert.Equal(2, initial.GeneratedTrees.Length);

        SyntaxTree oldTree = compilation.SyntaxTrees.Single(static tree => tree.FilePath.EndsWith("Parent.cs", StringComparison.Ordinal));
        SyntaxTree newTree = CSharpSyntaxTree.ParseText(oldTree.GetText(TestContext.Current.CancellationToken).ToString().Replace("public class Parent { }", "public class Parent { protected object _instance = new object(); }", StringComparison.Ordinal), (CSharpParseOptions)oldTree.Options, oldTree.FilePath, cancellationToken: TestContext.Current.CancellationToken);

        compilation = compilation.ReplaceSyntaxTree(oldTree, newTree);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generatedCompilation, out _, TestContext.Current.CancellationToken);
        GeneratorDriverRunResult updated = driver.GetRunResult();
        output.Write(updated);
        output.Write(generatedCompilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken));
        GeneratorRunResult result = Assert.Single(updated.Results);
        Assert.Null(result.Exception);
        Assert.Empty(result.Diagnostics);
        Assert.DoesNotContain(generatedCompilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken), static diagnostic => diagnostic.Severity is DiagnosticSeverity.Error);
        Assert.Equal(2, result.GeneratedSources.Length);
        Assert.Contains("_instance1 =", Assert.Single(result.GeneratedSources, static source => source.HintName == "Changed.StaticInstances.g.cs").SourceText.ToString());

        (object Value, IncrementalStepRunReason Reason)[] classes = [.. result.TrackedSteps["Classes"].SelectMany(static step => step.Outputs)];
        Assert.Equal(IncrementalStepRunReason.Unchanged, Assert.Single(classes, static entry => ((ClassInfo)entry.Value).Type.HintName == "Unchanged").Reason);
        Assert.Equal(IncrementalStepRunReason.Modified, Assert.Single(classes, static entry => ((ClassInfo)entry.Value).Type.HintName == "Changed").Reason);
        Assert.Equal(
            [IncrementalStepRunReason.Modified, IncrementalStepRunReason.Cached],
            result.TrackedOutputSteps["SourceOutput"].SelectMany(static step => step.Outputs).Select(static entry => entry.Reason).Order().ToArray());

        Assert.Equal(
            Assert.Single(initial.Results[0].GeneratedSources, static source => source.HintName == "Unchanged.StaticInstances.g.cs").SourceText.ToString(),
            Assert.Single(result.GeneratedSources, static source => source.HintName == "Unchanged.StaticInstances.g.cs").SourceText.ToString());
    }

    [Fact]
    public async Task MovingUnannotatedPartialDeclaration_PreservesCachedOutput()
    {
        var project = new MockProject(typeof(StaticInstancesGenerator).Assembly, cancellationToken: TestContext.Current.CancellationToken);
        _ = project.AddDocument("public partial class Service { }", "Declaration.cs");
        _ = project.AddDocument(
            """
            using Gori.StaticInstances;
            public partial class Service
            {
                [StaticInstance(Name = "Instance", Type = InstanceType.Lazy)]
                private Service() { }
            }
            """, "Instances.cs");

        Compilation compilation = await project.GetCompilationAsync();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new StaticInstancesGenerator().AsSourceGenerator()],
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        GeneratorDriverRunResult initial = driver.GetRunResult();
        output.Write(initial);
        Assert.Null(Assert.Single(initial.Results).Exception);
        GeneratedSourceResult initialSource = Assert.Single(initial.Results[0].GeneratedSources);

        SyntaxTree oldTree = compilation.SyntaxTrees.Single(static tree => tree.FilePath.EndsWith("Declaration.cs", StringComparison.Ordinal));
        SyntaxTree newTree = CSharpSyntaxTree.ParseText(
            "\n\n" + oldTree.GetText(TestContext.Current.CancellationToken).ToString(),
            (CSharpParseOptions)oldTree.Options,
            oldTree.FilePath,
            cancellationToken: TestContext.Current.CancellationToken);

        INamedTypeSymbol originalType = Assert.IsType<INamedTypeSymbol>(compilation.Assembly.GetTypeByMetadataName("Service"), exactMatch: false);
        compilation = compilation.ReplaceSyntaxTree(oldTree, newTree);
        INamedTypeSymbol movedType = Assert.IsType<INamedTypeSymbol>(compilation.Assembly.GetTypeByMetadataName("Service"), exactMatch: false);
        Assert.NotEqual(originalType.Locations[0].SourceSpan.Start, movedType.Locations[0].SourceSpan.Start);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generatedCompilation, out _, TestContext.Current.CancellationToken);
        GeneratorDriverRunResult updated = driver.GetRunResult();
        output.Write(updated);
        output.Write(generatedCompilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken));
        GeneratorRunResult result = Assert.Single(updated.Results);
        Assert.Null(result.Exception);
        Assert.Empty(result.Diagnostics);
        Assert.DoesNotContain(generatedCompilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken), static diagnostic => diagnostic.Severity is DiagnosticSeverity.Error);
        Assert.Equal(initialSource.SourceText.ToString(), Assert.Single(result.GeneratedSources).SourceText.ToString());
        Assert.All(result.TrackedOutputSteps["SourceOutput"].SelectMany(static step => step.Outputs), static entry => Assert.Equal(IncrementalStepRunReason.Cached, entry.Reason));

        var originalClass = (ClassInfo)Assert.Single(initial.Results[0].TrackedSteps["Classes"].SelectMany(static step => step.Outputs)).Value;
        var currentClass = (ClassInfo)Assert.Single(result.TrackedSteps["Classes"].SelectMany(static step => step.Outputs)).Value;
        Assert.Equal(originalClass, currentClass);
        Assert.Equal(originalClass.GetHashCode(), currentClass.GetHashCode());
    }

    private async Task<GeneratorRunResult> GenerateAndCheckCompilation(MockProject project)
    {
        Compilation compilation = await project.GetCompilationAsync();
        GeneratorDriver driver = project.CreateGeneratorDriver<StaticInstancesGenerator>();
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generatedCompilation, out _, project.CancellationToken);
        GeneratorDriverRunResult runResult = driver.GetRunResult();
        output.Write(runResult);
        output.Write(generatedCompilation.GetDiagnostics(cancellationToken: project.CancellationToken));
        GeneratorRunResult result = Assert.Single(runResult.Results);
        Assert.Null(result.Exception);
        Assert.Empty(result.Diagnostics);
        Assert.DoesNotContain(generatedCompilation.GetDiagnostics(cancellationToken: project.CancellationToken), static diagnostic => diagnostic.Severity is DiagnosticSeverity.Error);

        return result;
    }
}
