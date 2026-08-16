namespace Gori.StaticInstances;

/// <summary>
/// Common options shared by <see cref="SingletonAttribute"/> and <see cref="StaticInstanceAttribute"/>.
/// </summary>
public interface IStaticInstanceOptions
{
    /// <summary>
    /// Gets or sets the name that should be assigned to the static instance property.
    /// Must be a valid C# identifier that is not a reserved or contextual keyword.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of static instance to generate.
    /// </summary>
    InstanceType Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the accessibility modifier given to the static
    /// instance property should be internal, instead of the default public.
    /// </summary>
    bool Internal { get; set; }
}
