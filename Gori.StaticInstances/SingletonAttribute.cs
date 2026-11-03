using System;

namespace Gori.StaticInstances;

/// <summary>
/// Attribute to make class implement the singleton pattern.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SingletonAttribute : Attribute
{
    /// <summary>
    /// Default singleton name.
    /// </summary>
    public const string DefaultSingletonPropertyName = "Instance";

    /// <summary>
    /// Gets or sets the name that should be assigned to the static instance property.
    /// Must be a valid C# identifier that is not a reserved or contextual keyword.
    /// </summary>
    public string Name { get; set; } = DefaultSingletonPropertyName;

    /// <summary>
    /// Gets or sets the type of static instance to generate.
    /// </summary>
    public InstanceType Type { get; set; }

    /// <summary>
    /// Gets or sets the comment that should be added to the generated static instance property.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the accessibility modifier given to the static
    /// instance property should be internal, instead of the default public.
    /// </summary>
    public bool Internal { get; set; }

    /// <summary>
    /// Gets or sets when Singleton attribute should add a private default constructor.
    /// By default creates a private default constructor, if no other constructors exist.
    /// </summary>
    public Constructor Constructor { get; set; }
}
