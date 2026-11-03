// Ignore Spelling: gssi sgle

using System.Collections.Immutable;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;
using Gori.StaticInstances.CodeFix;
using Gori.StaticInstances.Testing.TestData;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Gori.StaticInstances.Testing;

public class AnalyzerTests(ITestOutputHelper output)
{
    [Theory]
    [ClassData(typeof(Analyzer_GSSI001_TestData))]
    public async Task EnforcePartialCodeFixTest(MockSourceData data)
        => await MockProject.AssertAnalyzerTests<StaticInstancesAnalyzer, EnforcePartialCodeFix>(data, output);

    [Theory]
    [ClassData(typeof(Analyzer_GSSI002_TestData))]
    [ClassData(typeof(Analyzer_GSSI003_TestData))]
    [ClassData(typeof(Analyzer_GSSI004_TestData))]
    [ClassData(typeof(Analyzer_GSSI005_TestData))]
    [ClassData(typeof(Analyzer_GSSI006_TestData))]
    [ClassData(typeof(Analyzer_GSSI007_TestData))]
    [ClassData(typeof(Analyzer_GSSI008_TestData))]
    [ClassData(typeof(Analyzer_GSSI009_TestData))]
    [ClassData(typeof(Analyzer_GSSI010_TestData))]
    public async Task StaticInstancesAnalyzerTest(MockSourceData data)
        => await MockProject.AssertAnalyzerTests<StaticInstancesAnalyzer>(data, output);

    [Fact]
    public async Task LanguageVersionTooLow_ReportsDiagnosticGSSI011()
    {
        const string source = """
            using Gori.StaticInstances;

            [Singleton]
            public partial class MyService { }
            """;

        var project = new MockProject(typeof(StaticInstancesAnalyzer).Assembly, languageVersion: LanguageVersion.CSharp10);
        _ = project.AddDocument(source);
        ImmutableArray<Diagnostic> diagnostics = await project.GetDiagnosticsAsync<StaticInstancesAnalyzer>();
        output.Write(diagnostics);

        Assert.Contains(diagnostics, d => d.Id == "GSSI011");
        Assert.Equal(DiagnosticSeverity.Error, diagnostics.First().Severity);
    }
}
