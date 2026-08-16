using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI001_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI001";
    public const string Category = $"Analyzer\\{ID}";

    public IEnumerator<TheoryDataRow<string, AnalyzerMatchIdData[], string?>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Both attributes checks
        // -------------------------------------------------------------------------

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public class MyService 
            {
                private MyService() { }

                [StaticIntance(1, Name = "Default")]
                public MyService(int value) { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 55, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on non-partial class with both [Singleton] and [StaticInstance]"
        };

        // -------------------------------------------------------------------------
        // Singleton checks
        // -------------------------------------------------------------------------

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public class MyService { }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 55, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on non-partial class with [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService { }
            """,
            [
                new AnalyzerMatchIdData() {ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on partial class with [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            public class Outer
            {
                [Singleton]
                public partial class Inner { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 42, AdditionalPositions = [], Contains = ["Outer"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on non-partial outer class with, nested partial class with [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            public class Outer
            {
                [Singleton]
                public class Inner { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 42, AdditionalPositions = [], Contains = ["Outer"], Severity = DiagnosticSeverity.Error },
                new AnalyzerMatchIdData() { ID = ID, Position = 86, AdditionalPositions = [], Contains = ["Inner"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on non-partial outer and inner classes when, nested class has [Singleton]"
        };

        yield return new("""
            using Gori.StaticInstances;
            public partial class Outer
            {
                [Singleton]
                public partial class Inner { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = -1 },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Not raised on partial outer or inner classes when, nested class has [Singleton]"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        yield return new("""
            using Gori.StaticInstances;
            public class MyService 
            {
                [StaticInstance(Name = "Instance")]
                public MyService() { }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 42, AdditionalPositions = [], Contains = ["MyService"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on non-partial class with [StaticInstance]"
        };

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
            Label = $"{ID}: Not raised on partial class with [StaticInstance]"
        };

        yield return new("""
            using Gori.StaticInstances;
            public class Outer
            {
                public partial class Inner 
                {
                    [StaticInstance(Name = "Instance")]
                    public Inner() { }
                }
            }
            """,
            [
                new AnalyzerMatchIdData() { ID = ID, Position = 42, AdditionalPositions = [], Contains = ["Outer"], Severity = DiagnosticSeverity.Error },
            ], Prefix)
        {
            Traits = { { "Category", [Category] } },
            Label = $"{ID}: Raised on non-partial outer class with, nested partial class with [StaticInstance]"
        };

        // -------------------------------------------------------------------------
        // No attribute checks
        // -------------------------------------------------------------------------

        yield return new("""
            public class MyService { }
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
