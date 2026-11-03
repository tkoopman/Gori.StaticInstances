using System.Collections;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI007_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public static readonly DiagnosticDescriptor Target = DiagnosticDescriptors.ArgumentCountMismatch;
    public static readonly HashSet<string> Category = [$"Analyzer\\{Target.Id}"];
    public static readonly DiagnosticExpectation NotTargetId = new DiagnosticExpectation()
    {
        IsExpected = false,
        Id = Target.Id,
    };

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {
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
                [StaticInstance(Name = "Default")]
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
            Label = $"{Target.Id}: Not raised on [StaticInstance] on default constructor"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode = """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(Name = "Default")]
                private MyService (object value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [],
                    Message = "0 argument(s)",
                },
                new DiagnosticExpectation()
                {
                    IsExpected = false,
                    Id = DiagnosticDescriptors.ArgumentTypeMismatch.Id,
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on [StaticInstance] when too few arguments supplied for constructor"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance("1", 2, Name = "Default")]
                private MyService (object value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [],
                    Message = "2 argument(s)",
                },
                new DiagnosticExpectation()
                {
                    IsExpected = false,
                    Id = DiagnosticDescriptors.ArgumentTypeMismatch.Id,
                },
            ],
        })
        {
            Traits = { { "Category", [.. Category, $"Analyzer\\{DiagnosticDescriptors.ArgumentTypeMismatch.Id}"] } },
            Label = $"{Target.Id}: Raised on [StaticInstance] when too many arguments supplied for constructor. {DiagnosticDescriptors.ArgumentTypeMismatch.Id} not raise"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(null, 2, Name = "Default")]
                private MyService (int value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [],
                    Message = "2 argument(s)",
                },
                new DiagnosticExpectation()
                {
                    IsExpected = false,
                    Id = DiagnosticDescriptors.ValueArgumentIsNull.Id,
                },
            ],
        })
        {
            Traits = { { "Category", [.. Category, $"Analyzer\\{DiagnosticDescriptors.ValueArgumentIsNull.Id}"] } },
            Label = $"{Target.Id}: Raised on [StaticInstance] when too many arguments supplied for constructor. {DiagnosticDescriptors.ValueArgumentIsNull.Id} not raise"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(Name = "Default")]
                public MyService () { }
            
                [StaticInstance(Name = "Named")]
                public MyService (string value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 140,
                    AdditionalPositions = [],
                    Message = "Named",
                },
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 140,
                    AdditionalPositions = [],
                    Message = "0 argument(s)",
                },
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 140,
                    AdditionalPositions = [],
                    Message = "1 parameter(s)",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on [StaticInstance] when too few arguments supplied for assigned constructor but another constructor matching exists"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(Name = "Named", null)]
                public MyService (object? value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [],
                    Message = "Named",
                },
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [],
                    Message = "0 argument(s)",
                },
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [],
                    Message = "1 parameter(s)",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on [StaticInstance] when too few arguments supplied for assigned constructor. [null] required"
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
