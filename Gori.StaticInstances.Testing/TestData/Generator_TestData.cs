using System.Collections;

namespace Gori.StaticInstances.Testing.TestData;

public class Generator_TestData : IEnumerable<TheoryDataRow<ISimpleTestData>>
{
    public const string TestPrefix = "Generator";

    public const string Category = "Generator";

    public IEnumerator<TheoryDataRow<ISimpleTestData>> GetEnumerator()
    {
        yield return new(new CSharpSyntaxTestData()
        {
            Input = """
                public partial class MyService { }
                """,
            Expected = string.Empty,
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{TestPrefix}: No attributes",
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
