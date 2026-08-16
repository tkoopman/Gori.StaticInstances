using System.Diagnostics.CodeAnalysis;

using Xunit.Sdk;

namespace Gori.StaticInstances.Testing;

/// <summary>
/// Treats all consecutive whitespace as single ' ' except at start or end of string, or before or after a new line.
/// Treats all consecutive end of line characters as a single '\n' except at start or end of string.
/// </summary>
public class AssertStringComparison : IEqualityComparer<string?>
{
    public bool IgnoreCase { get; set; }

    public static void Equal(string? expected, string? actual, bool ignoreCase = false)
    {
        if (ReferenceEquals(expected, actual))
        {
            return;
        }

        if (expected is null || actual is null)
        {
            return;
        }

        AssertEqualityResult result = Equivalent(expected.AsSpan(), actual.AsSpan(), ignoreCase);
        if (!result.Equal)
        {
            throw EqualException.ForMismatchedStrings(expected.ToString(), actual.ToString(), result.MismatchIndexX ?? -1, result.MismatchIndexY ?? -1);
        }
    }

    public static AssertEqualityResult Equivalent(
        ReadOnlySpan<char> expected,
        ReadOnlySpan<char> actual,
        bool ignoreCase = false)
    {
        int xI = -1;
        bool isXEnd = false;

        int yI = -1;
        bool isYEnd = false;

        while (true)
        {
            char expectedChar = FindNextValidChar(expected, ref xI, ref isXEnd, ignoreCase);
            char actualChar = FindNextValidChar(actual, ref yI, ref isYEnd, ignoreCase);

            if (isXEnd && isYEnd)
            {
                return AssertEqualityResult.ForResult(true, expected.ToString(), actual.ToString());
            }

            if (isXEnd || isYEnd)
            {
                return AssertEqualityResult.ForMismatch(expected.ToString(), actual.ToString(), xI, yI);
            }

            if (expectedChar != actualChar)
            {
                return AssertEqualityResult.ForMismatch(expected.ToString(), actual.ToString(), xI, yI);
            }
        }
    }

    public bool Equals(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        AssertEqualityResult result = Equivalent(x.AsSpan(), y.AsSpan(), IgnoreCase);
        return result.Equal;
    }

    public int GetHashCode([DisallowNull] string? obj)
    {
        if (obj is null)
        {
            return 0;
        }

        var hash = new HashCode();
        int x = -1;
        bool isEnd = false;

        while (true)
        {
            char c = FindNextValidChar(obj, ref x, ref isEnd, IgnoreCase);
            if (isEnd)
            {
                break;
            }

            hash.Add(c);
        }

        return hash.ToHashCode();
    }

    private static char FindNextValidChar(ReadOnlySpan<char> str, ref int i, ref bool isEnd, bool ignoreCase)
    {
        while (true)
        {
            if (++i >= str.Length)
            {
                i--;
                isEnd = true;
                return char.MinValue;
            }

            char c = str[i];
            bool isEOL = c is '\r' or '\n';
            bool isWhitespace = isEOL || char.IsWhiteSpace(c);
            if (isWhitespace)
            {
                for (i = i++; i < str.Length; i++)
                {
                    char t = str[i];
                    if (char.IsWhiteSpace(t))
                    {
                        continue;
                    }

                    if (t is '\r' or '\n')
                    {
                        isEOL = true;
                        continue;
                    }

                    i--;
                    return isEOL ? '\n' : ' ';
                }

                isEnd = true;
                return char.MinValue;
            }

            return ignoreCase ? char.ToUpperInvariant(str[i]) : str[i];
        }
    }
}
