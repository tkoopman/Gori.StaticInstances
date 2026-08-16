namespace Gori.StaticInstances;

/// <summary>
/// Attribute placed on a constructor to generate a named static property that creates
/// an instance of the class using that constructor with the supplied arguments.
/// Multiple attributes can be placed on the same constructor to produce several
/// named properties. All names must be unique within the class, including the name
/// used by <see cref="SingletonAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true, Inherited = false)]
public sealed class StaticInstanceAttribute : Attribute, IStaticInstanceOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StaticInstanceAttribute"/> class.
    /// </summary>
    /// <param name="args">The arguments to pass to the constructor.</param>
    public StaticInstanceAttribute(params object[] args)
    {
    }

    /// <inheritdoc/>
    public required string Name { get; set; }

    /// <inheritdoc/>
    public InstanceType Type { get; set; }

    /// <inheritdoc/>
    public bool Internal { get; set; }
}
