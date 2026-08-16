namespace Gori.StaticInstances.Testing.Examples;

/// <summary>
/// Most basic example.
/// </summary>
[Singleton]
public partial class SimpleExample
{
}

/*
 * Below is what generated code would be for this example.
 * 
 
    namespace Gori.StaticInstances.Testing.Examples
    {
        partial class SimpleExample
        {
            private SimpleExample () { }
            public static SimpleExample Instance { get; } = new();
        }
    }

 */
