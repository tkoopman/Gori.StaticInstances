namespace Gori.StaticInstances;

/// <summary>
/// Change how static instance will be generated.
/// </summary>
public enum InstanceType
{
    /// <summary>
    /// Instance will be created as a property.
    /// </summary>
    Default,

    /// <summary>
    /// Instance will be created as a property.
    /// </summary>
    Property = Default,

    /// <summary>
    /// Instance will be created as a field.
    /// </summary>
    Field,

    /// <summary>
    /// Property backed by <see cref="System.Lazy{T}"/>.
    /// </summary>
    Lazy,

    /// <summary>
    /// Property backed by <see cref="System.WeakReference{T}"/> with locking when needing to initialize new instance.
    /// </summary>
    Weak,

    /// <summary>
    /// Property backed by <see cref="System.WeakReference{T}"/>. Non-Thread-Safe version of <see cref="Weak"/>.
    /// </summary>
    WeakLockless,
}
