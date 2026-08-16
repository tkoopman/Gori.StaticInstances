using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI002_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI002";
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
                public MyService (int value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 30, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on class with no default constructor and [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.On)]
            public partial class MyService
            {
                public MyService (int value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on class with no default constructor but [Singleton(Constructor = Constructor.On)]"
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
            [Singleton]
            public partial class MyService () { }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], ID) // Using ID instead of prefix as will get GSSI003
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on class with primary default constructor and [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                private MyService () { }
                public MyService (int value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on class with multiple constructors including default constructor and [Singleton]"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        yield return new("""
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(1, Name = "Instance")]
                public MyService (int value) { }
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
            public partial class MyService
            {
                public MyService (int value) { }
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
