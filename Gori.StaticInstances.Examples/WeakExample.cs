namespace Gori.StaticInstances.Examples
{
    [Singleton(Type = InstanceType.WeakLockless)]
    public partial class WeakExample
    {
    }

    /*
     * Below is what generated code would be for this example.
     * 
 
        namespace Gori.StaticInstances.Testing.Examples
        {
            partial class WeakExample
            {
                private WeakExample () { }
                private static readonly WeakReference<WeakExample> weakInstance = new(null);
                public static WeakExample Instance
                {
                    get
                    {
                        if (weakInstance.TryGetTarget(out WeakExample currentInstance))
                        {
                            return currentInstance;
                        }
            
                        currentInstance = new();
                        weakInstance.SetTarget(currentInstance);
                        return currentInstance;
                    }
                }
            }
        }

     */
}
