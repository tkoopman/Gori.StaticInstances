using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI003_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI003";
    public const string Category = $"Analyzer\\{ID}";

    public IEnumerator<TheoryDataRow<string, AnalyzerMatchIdData[], string?>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Singleton checks
        // -------------------------------------------------------------------------

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                public MyService () { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 88, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Info },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on class with public default constructor and [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService () { }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 63, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Info },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on class with primary constructor and [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                internal MyService () { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 90, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Info },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on class with internal default constructor and [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                private MyService () { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on class with private default constructor and [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                protected MyService () { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on class with protected default constructor and [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService { }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on class with no explicit constructors and [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.Off)]
            public partial class MyService { }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 94, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Info },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on class with no explicit constructors and [Singleton(Constructor = false)]"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService 
            {
                [StaticInstance(Name = "Instance")]
                public MyService() { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on class with [StaticInstance]"
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
