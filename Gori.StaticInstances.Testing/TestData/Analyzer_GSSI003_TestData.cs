using System.Collections;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI003_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public static readonly DiagnosticDescriptor Target = DiagnosticDescriptors.NonPrivateDefaultConstructor;
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
                public MyService () { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 88,
                    AdditionalPositions = [],
                    Message = "MyService",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on class with public default constructor and [Singleton]"
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
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 63,
                    AdditionalPositions = [],
                    Message = "MyService",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on class with primary constructor and [Singleton]"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                internal MyService () { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 90,
                    AdditionalPositions = [],
                    Message = "MyService",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on class with internal default constructor and [Singleton]"
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
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on class with private default constructor and [Singleton]"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                protected MyService () { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on class with protected default constructor and [Singleton]"
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
            [Singleton(Constructor = Constructor.Off)]
            public partial class MyService { }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    Position = 94,
                    AdditionalPositions = [],
                    Message = "MyService",
                    IsExpected = true,
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on class with no explicit constructors and [Singleton(Constructor = false)]"
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
                [StaticInstance(Name = "Instance")]
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
            Label = $"{Target.Id}: Not raised on class with [StaticInstance]"
        };

        // -------------------------------------------------------------------------
        // No attribute checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
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
