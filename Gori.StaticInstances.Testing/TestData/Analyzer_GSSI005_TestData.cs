using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI005_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI005";
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
                    private MyService () { }
                
                    [StaticInstance(1, Name = "Instance")]
                    public MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 114, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on StaticInstance with name matching default Singleton's name"
        };

        yield return new("""
                using Gori.StaticInstances;
                [Singleton(Name = "Default")]
                public partial class MyService
                {
                    private MyService () { }
                
                    [StaticInstance(1, Name = "Default")]
                    public MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 132, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on StaticInstance with name matching custom Singleton's name"
        };

        yield return new("""
                using Gori.StaticInstances;
                [Singleton]
                public partial class MyService
                {
                    private MyService () { }
                
                    [StaticInstance(1, Name = "Default1")]
                    [StaticInstance(2, Name = "Default2")]
                    public MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised when StaticInstance and Singleton names are unique"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        // Should only raise on first duplicate name
        yield return new("""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(1, Name = "Instance")]
                    [StaticInstance(2, Name = "Instance")]
                    public MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 113, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on StaticInstance with 2x [StaticInstance] attributes with same name"
        };

        yield return new("""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(Name = "Instance")]
                    public MyService () { }

                    [StaticInstance(1, Name = "Instance")]
                    public MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 141, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on StaticInstance with 2x [StaticInstance] attributes on different constructors with same name"
        };

        // Should only raise on 1st and 2nd duplicate name
        yield return new("""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(1, Name = "Instance")]
                    [StaticInstance(2, Name = "Instance")]
                    [StaticInstance(3, Name = "Instance")]
                    public MyService (int value) { }
                }
                """,
                [
                new AnalyzerMatchIdData() { ID = ID, Position = 113, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
                new AnalyzerMatchIdData() { ID = ID, Position = 157, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
                ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on StaticInstance with 3x [StaticInstance] attributes with same name"
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
