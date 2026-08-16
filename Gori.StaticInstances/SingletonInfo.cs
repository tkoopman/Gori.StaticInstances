namespace Gori.StaticInstances;

public partial class SingletonGenerator
{
    private sealed class SingletonInfo(string instanceName, InstanceType instanceType, bool @internal, bool addConstructor, string constructorArgs) : IInstanceInfo
    {
        public string InstanceName { get; } = instanceName;

        public InstanceType InstanceType { get; } = instanceType;

        public bool Internal { get; } = @internal;

        public bool AddConstructor { get; } = addConstructor;

        public string ConstructorArgs { get; } = constructorArgs;
    }
}
