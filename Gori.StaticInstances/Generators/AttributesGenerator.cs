using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Generators;

/// <summary>
/// Source code generator for the attributes needed in consuming projects.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class AttributesGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterPostInitializationOutput(static postInitContext =>
        {
            postInitContext.AddEmbeddedAttributeDefinition();

            postInitContext.AddSource($"{Const.Namespace}.CompilerShims.g.cs", Const.CompilerShimsCode);
            postInitContext.AddSource($"{Const.Namespace}.{nameof(InstanceType)}.g.cs", Const.InstanceTypeCode);
            postInitContext.AddSource($"{Const.Namespace}.{nameof(Constructor)}.g.cs", Const.ConstructorCode);
            postInitContext.AddSource($"{Const.Namespace}.{nameof(SingletonAttribute)}.g.cs", Const.SingletonAttributeCode);
            postInitContext.AddSource($"{Const.Namespace}.{nameof(StaticInstanceAttribute)}.g.cs", Const.StaticInstanceAttributeCode);
        });
}
