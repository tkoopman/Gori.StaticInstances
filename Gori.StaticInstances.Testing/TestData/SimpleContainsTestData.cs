using Xunit.Sdk;

namespace Gori.StaticInstances.Testing.TestData;

public struct SimpleContainsTestData() : IXunitSerializable, ISimpleTestData
{
    /// <summary>
    /// Input string
    /// </summary>
    public required string Input { get; set; }

    /// <summary>
    /// Used to compare output to Contains and DoesNotContain
    /// </summary>
    public bool IgnoreCase { get; set; }

    /// <summary>
    /// If provided, must contain all of these strings
    /// </summary>
    public string[]? Contains { get; set; }

    /// <summary>
    /// If provided, result must not contain any of the contained strings
    /// </summary>
    public string[]? DoesNotContain { get; set; }

    public readonly void Assert(string? actual)
    {
        if (Contains is not null)
        {
            foreach (string? s in Contains)
            {
                Xunit.Assert.Contains(s, actual, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
        }

        if (DoesNotContain is not null)
        {
            foreach (string? s in DoesNotContain)
            {
                Xunit.Assert.DoesNotContain(s, actual, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
        }
    }

    public void Deserialize(IXunitSerializationInfo info)
    {
        Input = info.GetValue<string>(nameof(Input))!;
        IgnoreCase = info.GetValue<bool>(nameof(IgnoreCase));
        Contains = info.GetValue<string[]>(nameof(Contains));
        DoesNotContain = info.GetValue<string[]>(nameof(DoesNotContain));
    }

    public readonly void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue(nameof(Input), Input);
        info.AddValue(nameof(IgnoreCase), IgnoreCase);
        info.AddValue(nameof(Contains), Contains);
        info.AddValue(nameof(DoesNotContain), DoesNotContain);
    }
}
