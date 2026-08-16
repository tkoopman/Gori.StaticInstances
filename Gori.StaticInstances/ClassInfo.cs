using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances;

public partial class SingletonGenerator
{
    /// <summary>
    /// Stores class info including list of all defined <see cref="StaticInstanceAttribute"/> and <see cref="SingletonAttribute"/> attributes.
    /// </summary>
    private sealed class ClassInfo(INamedTypeSymbol classSymbol, string typeKind, string typeParameters, string @namespace, List<string> containingTypes, string hintName, List<IInstanceInfo> staticInstances)
    {
        public INamedTypeSymbol ClassSymbol { get; } = classSymbol;

        public string ClassName => ClassSymbol.Name;

        public string TypeKind { get; } = typeKind;

        public string TypeParameters { get; } = typeParameters;

        public string FullName => ClassName + TypeParameters;

        public string Namespace { get; } = @namespace;

        public List<string> ContainingTypes { get; } = containingTypes;

        public string HintName { get; } = hintName;

        public List<IInstanceInfo> Instances { get; } = staticInstances;
    }
}
