namespace Gori.StaticInstances;
#pragma warning disable IDE0001

/*
 * Following code should also be copied into Const.SingletonAttributeCode
 */

/// <summary>
/// Attribute placed on a constructor to generate a named static property that creates
/// an instance of the class using that constructor with the supplied arguments.
/// Multiple attributes can be placed on the same constructor to produce several
/// named properties. All names must be unique within the class, including the name
/// used by <see cref="SingletonAttribute"/>.
/// </summary>
[global::System.AttributeUsage(global::System.AttributeTargets.Constructor, AllowMultiple = true, Inherited = false)]
internal sealed class StaticInstanceAttribute : global::System.Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StaticInstanceAttribute"/> class.
    /// </summary>
    /// <param name="args">The arguments to pass to the constructor.</param>
    public StaticInstanceAttribute(params object[] args)
    {
    }

    /// <summary>
    /// Gets or sets the name that should be assigned to the static instance property.
    /// Must be a valid C# identifier that is not a reserved or contextual keyword.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of static instance to generate.
    /// </summary>
    public InstanceType Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the accessibility modifier given to the static
    /// instance property should be internal, instead of the default public.
    /// </summary>
    public bool Internal { get; set; }
}
