using Gori.Common.Roslyn;

namespace Gori.StaticInstances.Generators;

/// <summary>
/// Symbol-free, value-equatable description of a single generated static instance
/// (either a <see cref="SingletonAttribute"/> entry or a <see cref="StaticInstanceAttribute"/> entry).
/// Entries are grouped into a structurally equatable type model before emission.
/// Type locations do not affect equality; attribute positions remain relevant to member ordering.
/// </summary>
internal sealed record InstanceInfo
{
    /// <summary>
    /// Gets the symbol-free description of the containing type (namespace, nested partial chain, identity).
    /// </summary>
    public required EquatableTypeInfo Type { get; init; }

    /// <summary>
    /// Gets the name the generated static instance member will be called.
    /// </summary>
    public required string InstanceName { get; init; }

    /// <summary>
    /// Gets the type of instance that should be generated.
    /// </summary>
    public required InstanceType InstanceType { get; init; }

    /// <summary>
    /// Gets the comment that should be added to the generated static instance member.
    /// </summary>
    public required string? Comment { get; init; }

    /// <summary>
    /// Gets a value indicating whether the member should be <c>internal</c> instead of <c>public</c>.
    /// </summary>
    public required bool Internal { get; init; }

    /// <summary>
    /// Gets a value indicating whether a private parameterless constructor should be emitted.
    /// Only ever <see langword="true"/> for the singleton entry.
    /// </summary>
    public required bool AddConstructor { get; init; }

    /// <summary>
    /// Gets the C# literal code for the constructor's arguments.
    /// </summary>
    public required string ConstructorArgs { get; init; }

    /// <summary>
    /// Gets a value indicating whether this entry originates from a <see cref="SingletonAttribute"/>.
    /// </summary>
    public required bool IsSingleton { get; init; }

    /// <summary>
    /// Gets the attribute's source path for deterministic ordering across partial declarations.
    /// </summary>
    public required string SourcePath { get; init; }

    /// <summary>
    /// Gets the attribute's source offset for ordering within a document.
    /// </summary>
    public required int SourcePosition { get; init; }

    /// <inheritdoc/>
    public bool Equals(InstanceInfo? other)
        => other is not null
        && EquatableTypeInfo.IgnoreLocationComparer.Equals(Type, other.Type)
        && InstanceName == other.InstanceName
        && InstanceType == other.InstanceType
        && Comment == other.Comment
        && Internal == other.Internal
        && AddConstructor == other.AddConstructor
        && ConstructorArgs == other.ConstructorArgs
        && IsSingleton == other.IsSingleton
        && SourcePath == other.SourcePath
        && SourcePosition == other.SourcePosition;

    /// <inheritdoc/>
    public override int GetHashCode()
        => unchecked((EquatableTypeInfo.IgnoreLocationComparer.GetHashCode(Type) * 397) ^ InstanceName.GetHashCode());
}
