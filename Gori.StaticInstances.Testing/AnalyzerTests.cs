// Ignore Spelling: gssi sgle

using System.Collections.Immutable;
using System.Globalization;

using Gori.StaticInstances.Analyzers;
using Gori.StaticInstances.Testing.TestData;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gori.StaticInstances.Testing;

public class AnalyzerTests(ITestOutputHelper output)
{
    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        CSharpCompilation compilation  = TestHelpers.CreateCompilationWithGeneratedSources(source);
        CompilationWithAnalyzers withAnalyzers = compilation.WithAnalyzers([new StaticInstancesAnalyzer()]);
        return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    /// <summary>
    /// Test theory for analyzers that should return either no or a single diagnostic with a certain ID
    /// </summary>
    /// <param name="source">Source code to use</param>
    /// <param name="expectedDiagnostics">Details of each Diagnostic that should or should not be returned.</param>
    [Theory]
    [ClassData(typeof(Analyzer_GSSI001_TestData))]
    [ClassData(typeof(Analyzer_GSSI002_TestData))]
    [ClassData(typeof(Analyzer_GSSI003_TestData))]
    [ClassData(typeof(Analyzer_GSSI004_TestData))]
    [ClassData(typeof(Analyzer_GSSI005_TestData))]
    [ClassData(typeof(Analyzer_GSSI006_TestData))]
    [ClassData(typeof(Analyzer_GSSI007_TestData))]
    [ClassData(typeof(Analyzer_GSSI008_TestData))]
    [ClassData(typeof(Analyzer_GSSI009_TestData))]
    [ClassData(typeof(Analyzer_GSSI010_TestData))]
    public async Task ExpectDiagnosticTests(string source, AnalyzerMatchIdData[] expectedDiagnostics, string? fullCoveragePrefix)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        foreach (Diagnostic d in diagnostics.OrderBy(d => d.Location.SourceSpan.Start))
        {
            FileLinePositionSpan lineSpan = d.Location.GetLineSpan();
            output.WriteLine($"{d.Id} @[{d.Location.SourceSpan.Start}, {lineSpan.StartLinePosition.Line}, {lineSpan.StartLinePosition.Character}] {d.Severity}: {d.GetMessage(CultureInfo.InvariantCulture)}'");
            if (d.AdditionalLocations.Count > 0)
            {
                output.WriteLine("  > Additional Locations:");
                foreach (Location l in d.AdditionalLocations)
                {
                    lineSpan = l.GetLineSpan();
                    output.WriteLine($"        @[{l.SourceSpan.Start}, {lineSpan.StartLinePosition.Line}, {lineSpan.StartLinePosition.Character}]");
                }
            }
        }

        foreach (IGrouping<string, AnalyzerMatchIdData> idGroup in expectedDiagnostics.GroupBy(d => d.Index))
        {
            AnalyzerMatchIdData first = idGroup.First();
            bool expectedToExist = first.Position >= 0;

            if (expectedToExist)
            {
                foreach (AnalyzerMatchIdData diag in idGroup)
                {
                    _ = Assert.Single(diagnostics, d => diag.Equals(d));
                }

                continue;
            }

            // Should not be found
            foreach (AnalyzerMatchIdData diag in idGroup)
            {
                Assert.Empty(diagnostics.Where(d => diag.Equals(d)));
            }
        }

        // If full coverage for supplied IDs then confirm count returned match
        if (fullCoveragePrefix is not null)
        {
            int found = diagnostics.Count(d => d.Id.StartsWith(fullCoveragePrefix, StringComparison.Ordinal));
            int expected = expectedDiagnostics.Count(d => d.Position >= 0 && d.ID.StartsWith(fullCoveragePrefix, StringComparison.Ordinal));
            Assert.Equal(expected, found);
        }
    }

    [Fact]
    public async Task LanguageVersionTooLow_ReportsDiagnosticGSSI011()
    {
        const string source = """
            using Gori.StaticInstances;

            [Singleton]
            public partial class MyService { }
            """;

        // Not using CreateCompilationWithGeneratedSources as would generate errors that don't reflex real testing.
        CSharpCompilation compilation = TestHelpers.CreateCompilation(source, LanguageVersion.CSharp10);
        CompilationWithAnalyzers withAnalyzers = compilation.WithAnalyzers([new StaticInstancesAnalyzer()]);
        ImmutableArray<Diagnostic> diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);

        Assert.Contains(diagnostics, d => d.Id == "GSSI011");
        Assert.Equal(DiagnosticSeverity.Error, diagnostics.First().Severity);
    }
}
