using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Gori.StaticInstances.Testing;

internal static class TestHelpers
{

    /// <summary>
    /// Returns metadata references covering the full trusted platform assemblies list plus
    /// the Gori.StaticInstances assembly (which provides <see cref="SingletonAttribute"/>).
    /// Using TRUSTED_PLATFORM_ASSEMBLIES is the most reliable cross-platform way to get
    /// all standard-library references for an in-memory Roslyn compilation.
    /// </summary>
    public static IReadOnlyList<MetadataReference> GetBaseReferences()
    {
        string[] trustedPaths = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        var references = new List<MetadataReference>(trustedPaths.Length + 1);
        foreach (string path in trustedPaths)
        {
            references.Add(MetadataReference.CreateFromFile(path));
        }

        // Add the generator/attribute assembly itself (PrivateAssets="all" keeps it out of
        // transitive references, so we must add it explicitly).
        string singletonAssemblyPath = typeof(SingletonAttribute).Assembly.Location;
        if (!trustedPaths.Contains(singletonAssemblyPath, StringComparer.OrdinalIgnoreCase))
        {
            references.Add(MetadataReference.CreateFromFile(singletonAssemblyPath));
        }

        return references;
    }

    /// <summary>Creates an in-memory <see cref="CSharpCompilation"/> from a source string.</summary>
    public static CSharpCompilation CreateCompilation(string source)
        => CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(source)],
            GetBaseReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    /// <summary>
    /// Runs <see cref="SingletonGenerator"/> against <paramref name="source"/> and returns
    /// the concatenated text of all generated files, or <see cref="string.Empty"/> if nothing
    /// was generated. Use <see cref="RunGeneratorTrees"/> when you need per-file access.
    /// </summary>
    public static string RunGenerator(string source)
    {
        IReadOnlyList<string> trees = RunGeneratorTrees(source);
        return trees.Count > 0
            ? string.Concat(trees)
            : string.Empty;
    }

    /// <summary>
    /// Runs <see cref="SingletonGenerator"/> against <paramref name="source"/> and returns
    /// the text of each generated file as a separate entry, in generation order.
    /// </summary>
    public static IReadOnlyList<string> RunGeneratorTrees(string source)
    {
        CSharpCompilation compilation = CreateCompilation(source);
        var generator  = new SingletonGenerator();
        var driver     = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        GeneratorDriverRunResult result = driver.GetRunResult();
        return [.. result.GeneratedTrees.Select(t => t.GetText().ToString())];
    }
}
