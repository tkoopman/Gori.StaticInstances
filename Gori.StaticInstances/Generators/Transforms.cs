using System;
using System.Collections.Immutable;
using System.Linq;

using Gori.Common.Roslyn;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Generators;

/// <summary>
/// Contains transformations from Roslyn symbols to symbol-free <see cref="InstanceInfo"/>.
/// </summary>
internal class Transforms
{
    /// <summary>
    /// Produces the single singleton instance for a class annotated with <see cref="SingletonAttribute"/>.
    /// </summary>
    /// <param name="ctx">The <see cref="GeneratorAttributeSyntaxContext"/> containing the target type and its attributes.</param>
    /// <returns>
    /// The <see cref="InstanceInfo"/> representing the singleton instance, or <c>null</c> if the target symbol is not valid.
    /// </returns>
    public static InstanceInfo? GetSingletonInstance(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        AttributeData attr = ctx.Attributes[0];

        string instanceName       = attr.GetNamedArgumentAsString(nameof(SingletonAttribute.Name), Const.DefaultSingletonPropertyName);
        InstanceType instanceType = attr.GetNamedArgumentAsEnum<InstanceType>(nameof(SingletonAttribute.Type));
        string? comment           = attr.GetNamedArgumentAsString(nameof(SingletonAttribute.Comment));
        bool @internal            = attr.GetNamedArgumentAsBool(nameof(SingletonAttribute.Internal));
        Constructor constructor   = attr.GetNamedArgumentAsEnum<Constructor>(nameof(SingletonAttribute.Constructor));

        // Suppress the generated constructor if the user already declared any instance constructor.
        bool addConstructor = constructor switch
        {
            Constructor.Auto => !classSymbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared),
            Constructor.On => !classSymbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared && c.Parameters.IsEmpty),
            _ => false,
        };

        return BuildInstanceInfo(classSymbol, attr, instanceName, instanceType, comment, @internal, addConstructor, string.Empty, isSingleton: true);
    }

    /// <summary>
    /// Produces one <see cref="InstanceInfo"/> per <see cref="StaticInstanceAttribute"/> applied to the target constructor.
    /// </summary>
    /// <param name="ctx">The <see cref="GeneratorAttributeSyntaxContext"/> containing the target constructor and its attributes.</param>
    /// <returns>
    /// All static-instance entries on the constructor, or an empty array if the target symbol is not valid.
    /// </returns>
    public static ImmutableArray<InstanceInfo> GetStaticInstances(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not IMethodSymbol ctor || ctor.ContainingType is not INamedTypeSymbol classSymbol)
        {
            return [];
        }

        ImmutableArray<InstanceInfo>.Builder builder = ImmutableArray.CreateBuilder<InstanceInfo>();

        foreach (AttributeData siAttr in ctx.Attributes)
        {
            // Skip entries without a valid (non-whitespace) name; duplicate names are reported by the analyzer.
            if (siAttr.NamedArguments.FirstOrDefault(a => a.Key == nameof(StaticInstanceAttribute.Name)).Value.Value is not string siName
                || siName.AsSpan().IsWhiteSpace())
            {
                continue;
            }

            InstanceType siType = siAttr.GetNamedArgumentAsEnum<InstanceType>(nameof(StaticInstanceAttribute.Type));
            string? siComment   = siAttr.GetNamedArgumentAsString(nameof(StaticInstanceAttribute.Comment));
            bool siInternal     = siAttr.GetNamedArgumentAsBool(nameof(StaticInstanceAttribute.Internal));

            string siCtorArgs = string.Empty;
            if (siAttr.ConstructorArguments.Length >= 1)
            {
                TypedConstant argsConstant = siAttr.ConstructorArguments[0];
                if (!argsConstant.IsNull && argsConstant.Kind == TypedConstantKind.Array && !argsConstant.Values.IsDefaultOrEmpty)
                {
                    string[] parts = new string[argsConstant.Values.Length];
                    for (int i = 0; i < argsConstant.Values.Length; i++)
                    {
                        ITypeSymbol? paramType = i < ctor.Parameters.Length ? ctor.Parameters[i].Type : null;
                        parts[i] = SIHelper.FormatTypedConstant(argsConstant.Values[i], paramType);
                    }

                    siCtorArgs = string.Join(", ", parts);
                }
            }

            builder.Add(BuildInstanceInfo(classSymbol, siAttr, siName, siType, siComment, siInternal, addConstructor: false, siCtorArgs, isSingleton: false));
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Captures all symbol-dependent input needed for type-level emission.
    /// </summary>
    private static InstanceInfo BuildInstanceInfo(
        INamedTypeSymbol classSymbol,
        AttributeData attribute,
        string instanceName,
        InstanceType instanceType,
        string? comment,
        bool @internal,
        bool addConstructor,
        string constructorArgs,
        bool isSingleton)
    {
        EquatableTypeInfo typeInfo = classSymbol.CreateEquatableTypeInfo();

        instanceName = instanceName.AsSpan().IsWhiteSpace() ? Const.DefaultSingletonPropertyName : instanceName;
        SyntaxReference? source = attribute.ApplicationSyntaxReference;

        return new InstanceInfo
        {
            Type = typeInfo,
            InstanceName = instanceName,
            InstanceType = instanceType,
            Comment = comment,
            Internal = @internal,
            AddConstructor = addConstructor,
            ConstructorArgs = constructorArgs,
            IsSingleton = isSingleton,
            SourcePath = source?.SyntaxTree.FilePath ?? string.Empty,
            SourcePosition = source?.Span.Start ?? 0,
        };
    }
}
