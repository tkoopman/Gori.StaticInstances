using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI009_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI009";
    public const string Category = $"Analyzer\\{ID}";

    public IEnumerator<TheoryDataRow<string, AnalyzerMatchIdData[], string?>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance([null], Name = "Default")]
                private MyService (int value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = "GSSI008", Position = -1 },
                new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [], Contains = ["(int)default"], Severity = DiagnosticSeverity.Info },
            ], Prefix)
        {
            Traits = { { "Category", [Category, "Analyzer\\GSSI008"] } },
            Label = $"{ID}: Raised on [StaticInstance] when parameter is a value type set to null. GSSI008 not raised."
        };

        // -------------------------------------------------------------------------
        // No attribute checks
        // -------------------------------------------------------------------------

        yield return new("""
            public class MyService {
                public MyService() { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on class when no attributes used"
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
