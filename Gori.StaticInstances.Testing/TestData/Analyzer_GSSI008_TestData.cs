using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI008_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI008";
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
                [StaticInstance(1, Name = "Default1")]
                private MyService (int value) { }
            
                [StaticInstance(1, Name = "Default2")]
                private MyService(float value) { }
            
                [StaticInstance(1, Name = "Default3")]
                private MyService(string value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 154, AdditionalPositions = [], Contains = ["int", "float"], Severity = DiagnosticSeverity.Warning },
                new AnalyzerMatchIdData() { ID = ID, Position = 240, AdditionalPositions = [], Contains = ["int", "string"], Severity = DiagnosticSeverity.Warning },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on 2x [StaticInstance] when both have attribute types that do not match"
        };

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance([null], Name = "Default")]
                private MyService (object value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on [StaticInstance] when parameter is an object set to null. We do not enforce #nullable."
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
