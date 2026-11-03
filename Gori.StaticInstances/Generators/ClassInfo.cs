using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Gori.Common.Roslyn;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Generators;

/// <summary>
/// Symbol-free, structurally equatable input for one generated file per type.
/// Duplicate entries are retained until emission. Type locations are excluded from equality
/// because this model produces source, not location-dependent diagnostics.
/// </summary>
internal sealed record ClassInfo
{
    /// <summary>
    /// Gets the containing type's declaration and identity.
    /// </summary>
    public required EquatableTypeInfo Type { get; init; }

    /// <summary>
    /// Gets all instances, with the singleton first and static instances in source order.
    /// </summary>
    public required ImmutableArray<InstanceInfo> Instances { get; init; }

    /// <summary>
    /// Gets existing member names, including non-private inherited members and the type name.
    /// Captured once per type during grouping and compared structurally for caching.
    /// </summary>
    public required ImmutableArray<string> ExistingMemberNames { get; init; }

    /// <summary>
    /// Groups the two discovery streams without discarding any instance entries.
    /// </summary>
    /// <param name="singletons">Singleton entries.</param>
    /// <param name="staticInstances">Static-instance entries.</param>
    /// <param name="compilation">Current compilation used to resolve type-level collision metadata.</param>
    /// <returns>Type models in stable identity order.</returns>
    public static ImmutableArray<ClassInfo> Group(
        ImmutableArray<InstanceInfo> singletons,
        ImmutableArray<InstanceInfo> staticInstances,
        Compilation compilation)
        => [.. singletons.Concat(staticInstances)
                         .GroupBy(static instance => instance.Type, EquatableTypeInfo.IgnoreLocationComparer)
                         .OrderBy(static group => group.Key.MetadataName, StringComparer.Ordinal)
                         .Select(group => new ClassInfo
                         {
                             Type = group.Key,
                             ExistingMemberNames = GetExistingMemberNames(compilation, group.Key.MetadataName),
                             Instances = [.. group.OrderByDescending(static instance => instance.IsSingleton)
                                                  .ThenBy(static instance => instance.SourcePath, StringComparer.Ordinal)
                                                  .ThenBy(static instance => instance.SourcePosition),],
                         }),];

    /// <inheritdoc/>
    public bool Equals(ClassInfo? other)
        => other is not null
        && EquatableTypeInfo.IgnoreLocationComparer.Equals(Type, other.Type)
        && SequenceEquality.Equals(Instances, other.Instances)
        && SequenceEquality.Equals(ExistingMemberNames, other.ExistingMemberNames);

    /// <inheritdoc/>
    public override int GetHashCode()
        => unchecked((((EquatableTypeInfo.IgnoreLocationComparer.GetHashCode(Type) * 397) ^ SequenceEquality.GetHashCode(Instances)) * 397) ^ SequenceEquality.GetHashCode(ExistingMemberNames));

    /// <summary>
    /// Gets all members of type matching metadataName, including non-private members from base types.
    /// Returns unique list sorted for sequential equality.
    /// </summary>
    private static ImmutableArray<string> GetExistingMemberNames(Compilation compilation, string metadataName)
    {
        INamedTypeSymbol type = compilation.Assembly.GetTypeByMetadataName(metadataName)
            ?? throw new InvalidOperationException($"Cannot resolve discovered type '{metadataName}' in the current compilation.");

        var names = new HashSet<string>(StringComparer.Ordinal) { type.Name };

        // Include all members declared in the current type and all non-private members from base types.
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (SymbolEqualityComparer.Default.Equals(current, type) || member.DeclaredAccessibility is not Accessibility.Private)
                {
                    _ = names.Add(member.Name);
                }
            }
        }

        return [.. names.OrderBy(static name => name, StringComparer.Ordinal)];
    }
}
