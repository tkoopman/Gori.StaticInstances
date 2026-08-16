using System.Globalization;

using Microsoft.CodeAnalysis;

using Xunit.Sdk;

namespace Gori.StaticInstances.Testing.TestData;

public struct AnalyzerMatchIdData : IXunitSerializable, IEquatable<Diagnostic>
{
    public readonly string Index => Position >= 0 ? ID : $"-{ID}";

    /// <summary>
    /// ID of Diagnostic to look for
    /// </summary>
    public required string ID { get; set; }

    /// <summary>
    /// -1 if should not be returned, else character position that should be returned in <see cref="Diagnostic.Location"/>
    /// </summary>
    public required int Position { get; set; }

    /// <summary>
    /// If any additional locations should be returned and checked. Null doesn't check. Empty array confirms no additional locations.
    /// If provided all additional locations must be added and match for <see cref="Equals(Diagnostic?)"/>.
    /// </summary>
    public int[]? AdditionalPositions { get; set; }

    /// <summary>
    /// If provided, Diagnostic must contain each of these strings to match
    /// </summary>
    public string[]? Contains { get; set; }

    /// <summary>
    /// Severity Diagnostic should be.
    /// </summary>
    public DiagnosticSeverity Severity { get; set; }

    public void Deserialize(IXunitSerializationInfo info)
    {
        ID = info.GetValue<string>(nameof(ID)) ?? throw new ArgumentNullException(nameof(info), "ID must not be null");
        Position = info.GetValue<int>(nameof(Position));
        Contains = info.GetValue<string[]>(nameof(Contains));
        Severity = info.GetValue<DiagnosticSeverity>(nameof(Severity));
        AdditionalPositions = info.GetValue<int[]?>(nameof(AdditionalPositions));
    }

    public readonly void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue(nameof(ID), ID);
        info.AddValue(nameof(Position), Position);
        info.AddValue(nameof(Contains), Contains);
        info.AddValue(nameof(Severity), Severity);
        info.AddValue(nameof(AdditionalPositions), AdditionalPositions);
    }

    public readonly bool Equals(Diagnostic? diagnostic)
    {
        if (diagnostic is null || !ID.Equals(diagnostic.Id, StringComparison.Ordinal) || (Position >= 0 && Position != diagnostic.Location.SourceSpan.Start) || Severity != diagnostic.Severity)
        {
            return false;
        }

        if (AdditionalPositions is not null)
        {
            IOrderedEnumerable<int> expected = AdditionalPositions.Order();
            IOrderedEnumerable<int> actual = diagnostic.AdditionalLocations.Select(l => l.SourceSpan.Start).Order();
            Assert.Equal(expected, actual);
        }

        if (Contains is null || Contains.Length == 0)
        {
            return true;
        }

        string msg = diagnostic.GetMessage(CultureInfo.InvariantCulture);
        foreach (string s in Contains)
        {
            if (!msg.Contains(s))
            {
                return false;
            }
        }

        return true;
    }
}
