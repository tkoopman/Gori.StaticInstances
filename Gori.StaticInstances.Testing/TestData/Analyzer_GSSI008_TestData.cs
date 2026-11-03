using System.Collections;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI008_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public static readonly DiagnosticDescriptor Target = DiagnosticDescriptors.ArgumentTypeMismatch;
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
                [StaticInstance(1, Name = "Default1")]
                private MyService (int value) { }
            
                [StaticInstance(1, Name = "Default2")]
                private MyService(float value) { }
            
                [StaticInstance(1, Name = "Default3")]
                private MyService(string value) { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 154,
                    AdditionalPositions = [],
                    Message = "int",
                },
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 154,
                    AdditionalPositions = [],
                    Message = "float",
                },
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 240,
                    AdditionalPositions = [],
                    Message = "int",
                },
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 240,
                    AdditionalPositions = [],
                    Message = "string",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on 2x [StaticInstance] when both have attribute types that do not match"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance([null], Name = "Default")]
                private MyService (object value) { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on [StaticInstance] when parameter is an object set to null. We do not enforce #nullable."
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
