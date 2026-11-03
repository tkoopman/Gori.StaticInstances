using System.Collections;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI004_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public static readonly DiagnosticDescriptor Target = DiagnosticDescriptors.InvalidInstanceName;
    public static readonly HashSet<string> Category = [$"Analyzer\\{Target.Id}"];
    public static readonly DiagnosticExpectation NotTargetId = new DiagnosticExpectation()
    {
        IsExpected = false,
        Id = Target.Id,
    };

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

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Singleton checks
        // -------------------------------------------------------------------------

        foreach (string name in ValidNames)
        {
            yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
            {
                SourceCode =
                $$"""
                using Gori.StaticInstances;
                [Singleton(Name = "{{name}}")]
                public partial class MyService { }
                """,
                Expectations =
                [
                    NotTargetId,
                ],
            })
            {
                Traits = { { "Category", Category } },
                Label = $"{Target.Id}: Not raised on Singleton with valid name of '{name}'"
            };
        }

        foreach (string name in InvalidNames)
        {
            yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
            {
                SourceCode =
                $$"""
                using Gori.StaticInstances;
                [Singleton(Name = "{{name}}")]
                public partial class MyService { }
                """,
                Expectations =
                [
                    new DiagnosticExpectation(Target)
                    {
                        IsExpected = true,
                        Position = 30,
                        AdditionalPositions = [],
                        Message = $"'{name}'",
                    },
                ],
            })
            {
                Traits = { { "Category", Category } },
                Label = $"{Target.Id}: Raised on Singleton with invalid name of '{name}'"
            };
        }

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService { }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on Singleton with no explicit name"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        foreach (string name in ValidNames)
        {
            yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
            {
                SourceCode =
                $$"""
                using Gori.StaticInstances;
                public partial class MyService 
                {
                    [StaticInstance(Name = "{{name}}")]
                    public MyService () { }
                }
                """,
                Expectations =
                [
                    NotTargetId,
                ],
            })
            {
                Traits = { { "Category", Category } },
                Label = $"{Target.Id}: Not raised on StaticInstance with valid name of '{name}'"
            };
        }

        foreach (string name in InvalidNames)
        {
            yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
            {
                SourceCode =
                $$"""
                using Gori.StaticInstances;
                public partial class MyService
                {
                    [StaticInstance(Name = "{{name}}")]
                    public MyService () { }
                }
                """,
                Expectations =
                [
                    new DiagnosticExpectation(Target)
                    {
                        IsExpected = true,
                        Position = 69,
                        AdditionalPositions = [],
                        Message = $"'{name}'",
                    },
                ],
            })
            {
                Traits = { { "Category", Category } },
                Label = $"{Target.Id}: Raised on StaticInstance with invalid name of '{name}'"
            };
        }

        // -------------------------------------------------------------------------
        // No attribute checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            $$"""
            public class MyService {
                public MyService() { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on class when no attributes used"
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
