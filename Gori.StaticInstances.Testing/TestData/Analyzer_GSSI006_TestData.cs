using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI006_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI006";
    public const string Category = $"Analyzer\\{ID}";

    public IEnumerator<TheoryDataRow<string, AnalyzerMatchIdData[], string?>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Both attributes checks
        // -------------------------------------------------------------------------

        yield return new("""
                using Gori.StaticInstances;
                [Singleton]
                public partial class MyService
                {
                    [StaticInstance(Name = "Default")]
                    private MyService () { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 30, AdditionalPositions = [82, 129], Contains = ["MyService"], Severity = DiagnosticSeverity.Warning },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on [StaticInstance] on default constructor with [Singleton]"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        yield return new("""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(1, Name = "Default1")]
                    [StaticInstance(1, Name = "Default2")]
                    private MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [113, 164], Contains = ["MyService"], Severity = DiagnosticSeverity.Warning },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on 2x [StaticInstance] with same arguments"
        };

        yield return new("""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance("localhost", 5432, Name = "Dev")]
                    [StaticInstance("localhost", 5432, Name = "Dev2")]
                    public MyService (string host, int port) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [124, 186], Contains = ["MyService"], Severity = DiagnosticSeverity.Warning },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on 2x [StaticInstance] with same arguments across 2 parameters"
        };

        yield return new("""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(1, Name = "Default1")]
                    [StaticInstance(1.0f, Name = "Default2")]
                    private MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [113, 167], Contains = ["MyService"], Severity = DiagnosticSeverity.Warning },
                new AnalyzerMatchIdData() { ID = "GSSI008", Position = 113, AdditionalPositions = [], Contains = ["Default2"], Severity = DiagnosticSeverity.Warning },
                ], Prefix)
        {
            Traits = { { "Category", [Category, "Analyzer\\GSSI008"] } },
            Label = $"{ID}: Raised on 2x [StaticInstance] with same arguments after converting to match parameter type. GSSI008 also raised."
        };

        yield return new("""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(1, Name = "Default1")]
                    [StaticInstance(2, Name = "Default2")]
                    private MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on 2x [StaticInstance] with different arguments"
        };

        yield return new("""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(1, Name = "Default1")]
                    private MyService (int value) { }
                
                    [StaticInstance(1.0f, Name = "Default2")]
                    private MyService(float value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on 2x [StaticInstance] with different arguments"
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
