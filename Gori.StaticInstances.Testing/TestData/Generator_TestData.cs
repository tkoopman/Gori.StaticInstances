using System.Collections;

using Gori.Common.Roslyn.Test;

namespace Gori.StaticInstances.Testing.TestData;

public class Generator_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public const string TestPrefix = "Generator";

    public const string Category = "Generator";

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {
        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            public partial class MyService { }
            """,
            Expectations = [FileExpectation.NoOthers],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{TestPrefix}: No attributes",
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
