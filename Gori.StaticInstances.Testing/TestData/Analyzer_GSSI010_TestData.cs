using System.Collections;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI010_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public static readonly DiagnosticDescriptor Target = DiagnosticDescriptors.StaticInstanceOnPrimaryConstructor;
    public static readonly HashSet<string> Category = [$"Analyzer\\{Target.Id}"];
    public static readonly DiagnosticExpectation NotTargetId = new DiagnosticExpectation()
    {
        IsExpected = false,
        Id = Target.Id,
    };

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {
        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [method: StaticInstance(Name = "Instance")]
            public partial class MyService() { }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 38,
                    AdditionalPositions = [],
                    Message = "MyService",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on class with [StaticInstance] on primary constructor"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            [method: StaticInstance(Name = "MyInstance")]
            public partial class MyService() { }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 51,
                    AdditionalPositions = [],
                    Message = "MyService",
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on class with [StaticInstance] on primary constructor even if Singleton also used (Technically works)"
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
