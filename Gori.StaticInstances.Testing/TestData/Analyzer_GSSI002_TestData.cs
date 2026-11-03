using System.Collections;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI002_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public static readonly DiagnosticDescriptor Target = DiagnosticDescriptors.NoDefaultConstructor;
    public static readonly HashSet<string> Category = [$"Analyzer\\{Target.Id}"];
    public static readonly DiagnosticExpectation NotTargetId = new DiagnosticExpectation()
    {
        IsExpected = false,
        Id = Target.Id,
    };

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Singleton checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                public MyService (int value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 30,
                    AdditionalPositions = [],
                    Message = "MyService",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on class with no default constructor and [Singleton]"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.On)]
            public partial class MyService
            {
                public MyService (int value) { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on class with no default constructor but [Singleton(Constructor = Constructor.On)]"
        };

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
            Label = $"{Target.Id}: Not raised on class with no explicit constructors and [Singleton]"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService () { }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on class with primary default constructor and [Singleton]"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                private MyService () { }
                public MyService (int value) { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on class with multiple constructors including default constructor and [Singleton]"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(1, Name = "Instance")]
                public MyService (int value) { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on class with [StaticInstance]"
        };

        // -------------------------------------------------------------------------
        // No attribute checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            public partial class MyService
            {
                public MyService (int value) { }
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
