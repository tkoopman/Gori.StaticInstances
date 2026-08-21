namespace Gori.StaticInstances;

/*
 * Following code should also be copied into Const.ConstructorCode
 */

/// <summary>
/// Defines if Singleton attribute should generate
/// a private default constructor if a default constructor doesn't already exist.
/// </summary>
internal enum Constructor
{
    /// <summary>
    /// If no explicitly defined constructors exist,
    /// will add private default constructor.
    /// </summary>
    Auto,

    /// <summary>
    /// Will add private default constructor,
    /// unless a default constructor already exist.
    /// </summary>
    On,

    /// <summary>
    /// Will never add a constructor.
    /// </summary>
    Off,
}
