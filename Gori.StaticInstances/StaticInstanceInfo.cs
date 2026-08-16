namespace Gori.StaticInstances;

public partial class SingletonGenerator
{
    private sealed class StaticInstanceInfo(string instanceName, InstanceType instanceType, bool @internal, string constructorArgs) : IInstanceInfo
    {
        public string InstanceName { get; } = instanceName;

        public InstanceType InstanceType { get; } = instanceType;

        public bool Internal { get; } = @internal;

        public string ConstructorArgs { get; } = constructorArgs;

        bool IInstanceInfo.AddConstructor => false;
    }
}
