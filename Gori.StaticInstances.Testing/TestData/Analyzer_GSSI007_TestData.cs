using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI007_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI007";
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
                [StaticInstance(Name = "Default")]
                public MyService () { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on [StaticInstance] on default constructor"
        };

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(Name = "Default")]
                private MyService (object value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = "GSSI008", Position = -1 },
                new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [], Contains = ["0 argument"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on [StaticInstance] when too few arguments supplied for constructor"
        };

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance("1", 2, Name = "Default")]
                private MyService (object value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = "GSSI008", Position = -1 },
                new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [], Contains = ["2 argument"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category, "Analyzer\\GSSI008"] } },
            Label = $"{ID}: Raised on [StaticInstance] when too many arguments supplied for constructor. GSSI008 not raise"
        };

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(null, 2, Name = "Default")]
                private MyService (int value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = "GSSI009", Position = -1 },
                new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [], Contains = ["2 argument"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category, "Analyzer\\GSSI009"] } },
            Label = $"{ID}: Raised on [StaticInstance] when too many arguments supplied for constructor. GSSI009 not raise"
        };

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(Name = "Default")]
                public MyService () { }
            
                [StaticInstance(Name = "Named")]
                public MyService (string value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 140, AdditionalPositions = [], Contains = ["Named", "0 argument(s)", "1 parameter(s)"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on [StaticInstance] when too few arguments supplied for assigned constructor but another constructor matching exists"
        };

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(Name = "Named", null)]
                public MyService (object? value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [], Contains = ["Named", "0 argument(s)", "1 parameter(s)"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on [StaticInstance] when too few arguments supplied for assigned constructor. [null] required"
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
