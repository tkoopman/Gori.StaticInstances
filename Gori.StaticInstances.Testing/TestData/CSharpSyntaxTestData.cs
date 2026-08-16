using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit.Sdk;

namespace Gori.StaticInstances.Testing.TestData;

public struct CSharpSyntaxTestData() : IXunitSerializable, ISimpleTestData
{
    /// <summary>
    /// Input string
    /// </summary>
    public required string Input
    {
        get; set => field = CSharpSyntaxTree.ParseText(value).GetRoot().NormalizeWhitespace().ToFullString();
    }

    /// <summary>
    /// If provided, must contain all of these strings
    /// </summary>
    public required string Expected
    {
        get; set => field = CSharpSyntaxTree.ParseText(value).GetRoot().NormalizeWhitespace().ToFullString();
    }

    public bool NormalizeActual { get; set; } = true;

    public readonly void Assert(string? actual)
    {
        if (actual is null)
        {
            if (!string.IsNullOrEmpty(Expected))
            {
                throw EqualException.ForMismatchedStrings(Expected, actual, -1, -1);
            }

            return;
        }

        if (NormalizeActual)
        {
            actual = CSharpSyntaxTree.ParseText(actual).GetRoot().NormalizeWhitespace().ToFullString();
        }

        Xunit.Assert.Equal(Expected, actual);
    }

    public void Deserialize(IXunitSerializationInfo info)
    {
        Input = info.GetValue<string>(nameof(Input))!;
        Expected = info.GetValue<string?>(nameof(Expected)) ?? throw new ArgumentNullException(nameof(info), "Expected must not be null");
    }

    public readonly void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue(nameof(Input), Input);
        info.AddValue(nameof(Expected), Expected);
    }
}
