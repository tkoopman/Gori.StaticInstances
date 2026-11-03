using System.Collections;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI006_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public static readonly DiagnosticDescriptor Target = DiagnosticDescriptors.IdenticalInstances;
    public static readonly HashSet<string> Category = [$"Analyzer\\{Target.Id}"];
    public static readonly DiagnosticExpectation NotTargetId = new DiagnosticExpectation()
    {
        IsExpected = false,
        Id = Target.Id,
    };

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Both attributes checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                [StaticInstance(Name = "Default")]
                private MyService () { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 30,
                    AdditionalPositions = [82, 129],
                    Message = "MyService",
                }
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on [StaticInstance] on default constructor with [Singleton]"
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
                [StaticInstance(1, Name = "Default1")]
                [StaticInstance(1, Name = "Default2")]
                private MyService (int value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [113, 164],
                    Message = "MyService",
                }
            ]
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on 2x [StaticInstance] with same arguments"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance("localhost", 5432, Name = "Dev")]
                [StaticInstance("localhost", 5432, Name = "Dev2")]
                public MyService (string host, int port) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [124, 186],
                    Message = "MyService",
                }
            ]
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on 2x [StaticInstance] with same arguments across 2 parameters"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(1, Name = "Default1")]
                [StaticInstance(1.0f, Name = "Default2")]
                private MyService (int value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 69,
                    AdditionalPositions = [113, 167],
                    Message = "MyService",
                },
                new DiagnosticExpectation(DiagnosticDescriptors.ArgumentTypeMismatch)
                {
                    IsExpected = true,
                    Position = 113,
                    AdditionalPositions = [],
                    Message = "Default2",
                },
            ],
        })
        {
            Traits = { { "Category", [.. Category, "Analyzer\\GSSI008"] } },
            Label = $"{Target.Id}: Raised on 2x [StaticInstance] with same arguments after converting to match parameter type. {DiagnosticDescriptors.ArgumentTypeMismatch.Id} also raised."
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(1, Name = "Default1")]
                [StaticInstance(2, Name = "Default2")]
                private MyService (int value) { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on 2x [StaticInstance] with different arguments"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(1, Name = "Default1")]
                private MyService (int value) { }
                
                [StaticInstance(1.0f, Name = "Default2")]
                private MyService(float value) { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on 2x [StaticInstance] with different arguments"
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
