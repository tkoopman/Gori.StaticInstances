namespace Gori.StaticInstances.Testing.Examples;

[Singleton(Type = InstanceType.Weak)]
public partial class WeakThreadSafeExample
{
}

/*
 * Below is what generated code would be for this example.
 * 
 
    namespace Gori.StaticInstances.Testing.Examples
    {
        partial class WeakThreadSafeExample
        {
            private WeakThreadSafeExample () { }
    #if NET9_0_OR_GREATER
            private static readonly Lock _weakLock = new();
    #else
            private static readonly object _weakLock = new();
    #endif
            private static readonly WeakReference<WeakThreadSafeExample> weakInstance = new(null);
            public static WeakThreadSafeExample Instance
            {
                get
                {
                    if (weakInstance.TryGetTarget(out WeakThreadSafeExample currentInstance))
                    {
                        return currentInstance;
                    }
            
                    lock(_weakLock)
                    {
                        if (weakInstance.TryGetTarget(out currentInstance))
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
