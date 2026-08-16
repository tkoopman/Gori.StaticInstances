using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI010_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI010";
    public const string Category = $"Analyzer\\{ID}";

    public IEnumerator<TheoryDataRow<string, AnalyzerMatchIdData[], string?>> GetEnumerator()
    {
        yield return new("""
            using Gori.StaticInstances;
            [method: StaticInstance(Name = "Instance")]
            public partial class MyService() { }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 38, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on class with [StaticInstance] on primary constructor"
        };

        // While technically will work due to using Pipeline 1, should still raise for consistency
        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            [method: StaticInstance(Name = "MyInstance")]
            public partial class MyService() { }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 51, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
            ], ID) // Using ID ass get lots of other errors in this case
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on class with [StaticInstance] on primary constructor even if Singleton also used (Technically works)"
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
