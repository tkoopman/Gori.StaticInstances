namespace Gori.StaticInstances;

/// <summary>
/// Attribute to make class implement the singleton pattern.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SingletonAttribute : Attribute, IStaticInstanceOptions
{
    /// <inheritdoc/>
    public string Name { get; set; } = Const.DefaultSingletonPropertyName;

    /// <inheritdoc/>
    public InstanceType Type { get; set; }

    /// <inheritdoc/>
    public bool Internal { get; set; }

    /// <summary>
    /// Gets or sets when Singleton attribute should add a private default constructor.
    /// By default creates a private default constructor, if no other constructors exist.
    /// </summary>
    public Constructor Constructor { get; set; }
}
