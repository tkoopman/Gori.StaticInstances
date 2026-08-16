using System.Collections;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI004_TestData : IEnumerable<TheoryDataRow<string, AnalyzerMatchIdData[], string?>>
{
    public const string Prefix = "GSSI";
    public const string ID = "GSSI004";
    public const string Category = $"Analyzer\\{ID}";

    public static readonly string[] ValidNames =
        [
        "Instance",
        "DefaultInstance",
        "Default", // Not reserved as title case
        ];

    public static readonly string[] InvalidNames =
        [
        "", // Empty
        "1Instance", // Invalid starting char
        "Default Instance", // Invalid char
        "Default.Instance", // Invalid char
        "Default-Instance", // Invalid char
        "default", // Reserved word
        ];

    public IEnumerator<TheoryDataRow<string, AnalyzerMatchIdData[], string?>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Singleton checks
        // -------------------------------------------------------------------------

        foreach (string name in ValidNames)
        {
            yield return new($$"""
                using Gori.StaticInstances;
                [Singleton(Name = "{{name}}")]
                public partial class MyService { }
                """,
                [
                    new AnalyzerMatchIdData() { ID = ID, Position = -1 },
                ], Prefix)
            {
                Traits = { { "Category", [Category] } },
                Label = $"{ID}: Not raised on Singleton with valid name of '{name}'"
            };
        }

        foreach (string name in InvalidNames)
        {
            yield return new($$"""
                using Gori.StaticInstances;
                [Singleton(Name = "{{name}}")]
                public partial class MyService { }
                """,
                [
                    new AnalyzerMatchIdData() { ID = ID, Position = 30, AdditionalPositions = [], Contains = [$"'{name}'"], Severity = DiagnosticSeverity.Error },
                ], Prefix)
            {
                Traits = { { "Category", [Category] } },
                Label = $"{ID}: Raised on Singleton with invalid name of '{name}'"
            };
        }

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
            Label = $"{ID}: Not raised on Singleton with no explicit name"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        foreach (string name in ValidNames)
        {
            yield return new($$"""
                using Gori.StaticInstances;
                public partial class MyService 
                {
                    [StaticInstance(Name = "{{name}}")]
                    public MyService () { }
                }
                """,
                [
                    new AnalyzerMatchIdData() { ID = ID, Position = -1 },
                ], Prefix)
            {
                Traits = { { "Category", [Category] } },
                Label = $"{ID}: Not raised on StaticInstance with valid name of '{name}'"
            };
        }

        foreach (string name in InvalidNames)
        {
            yield return new($$"""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(Name = "{{name}}")]
                    public MyService () { }
                }
                """,
                [
                    new AnalyzerMatchIdData() { ID = ID, Position = 69, AdditionalPositions = [], Contains = [$"'{name}'"], Severity = DiagnosticSeverity.Error },
                ], Prefix)
            {
                Traits = { { "Category", [Category] } },
                Label = $"{ID}: Raised on StaticInstance with invalid name of '{name}'"
            };
        }

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
