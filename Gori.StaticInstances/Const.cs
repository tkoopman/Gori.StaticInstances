namespace Gori.StaticInstances;

#pragma warning disable SA1600 // Elements should be documented
internal static class Const
{
    public const string Namespace = "Gori.StaticInstances";
    public const string DefaultSingletonPropertyName = "Instance";
    public const string SingletonAttributeShortName = "Singleton";
    public const string SingletonAttributeName = "SingletonAttribute";
    public const string SingletonAttributeFullName = $"{Namespace}.{SingletonAttributeName}";
    public const string StaticInstanceAttributeShortName = "StaticInstance";
    public const string StaticInstanceAttributeName = "StaticInstanceAttribute";
    public const string StaticInstanceAttributeFullName = $"{Namespace}.{StaticInstanceAttributeName}";
    public const string DiagnosticDescriptorCategory = Namespace;
    public const string DiagnosticDescriptorIDPrefix = "GSSI";
}
