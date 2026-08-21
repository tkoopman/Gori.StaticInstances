namespace Gori.StaticInstances;

public partial class StaticInstancesGenerator
{
    /// <summary>
    /// Data representing a Singleton or StaticInstance parameters.
    /// </summary>
    private interface IInstanceInfo
    {
        /// <summary>
        /// Gets the name that the static instance property will be called.
        /// </summary>
        string InstanceName { get; }

        /// <summary>
        /// Gets the type of instance that should be generated.
        /// </summary>
        InstanceType InstanceType { get; }

        /// <summary>
        /// Gets a value indicating whether the accessibility modifier given to the static
        /// instance property should be internal, instead of the default public.
        /// </summary>
        bool Internal { get; }

        /// <summary>
        /// Gets a value indicating whether a private static default constructor should be added.
        /// </summary>
        bool AddConstructor { get; }

        /// <summary>
        /// Gets C# literal code for constructor's arguments.
        /// </summary>
        string ConstructorArgs { get; }
    }
}
