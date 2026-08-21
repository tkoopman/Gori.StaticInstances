namespace Gori.StaticInstances.Examples
{
    /// <summary>
    /// Example showing that if you manually define a default constructor,
    /// the source generator will see this and not add the automatic private constructor.
    /// NOTE: The automatic constructor will not be created if ANY custom constructor exists.
    /// </summary>
    [Singleton]
    public partial class SimpleCustomConstructorExample
    {
        private SimpleCustomConstructorExample()
        {
            // Execute code here
        }
    }

    /*
     * Below is what generated code would be for this example.
     * 
 
        namespace Gori.StaticInstances.Testing.Examples
        {
            partial class SimpleCustomConstructorExample
            {
                public static SimpleCustomConstructorExample Instance { get; } = new();
            }
        }

     */
}
