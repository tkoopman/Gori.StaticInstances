using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using csb = Gori.CodeStringBuilder.CodeStringBuilder;

namespace Gori.StaticInstances;

/// <summary>
/// Source code generator for both <see cref="SingletonAttribute"/> and <see cref="StaticInstanceAttribute"/>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public partial class SingletonGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Pipeline 1 – classes annotated with [Singleton] (also collects any [StaticInstance] constructors)
        IncrementalValuesProvider<ClassInfo> singletonInfo = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Const.SingletonAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetPipeline1ClassInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Pipeline 2 – classes that have [StaticInstance] on constructors but no [Singleton] on the class.
        // getPipeline2ClassInfo will only return value for first StaticInstance in class, so no need to de-dupe.
        IncrementalValuesProvider<ClassInfo> staticOnlyInfo = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Const.StaticInstanceAttributeFullName,
                predicate: static (node, _) => node is ConstructorDeclarationSyntax,
                transform: static (ctx, _) => GetPipeline2ClassInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        context.RegisterSourceOutput(singletonInfo, static (spc, info) => Execute(spc, info));
        context.RegisterSourceOutput(staticOnlyInfo, static (spc, info) => Execute(spc, info));
    }

    /// <summary>
    /// Handles classes that has Singleton attribute.
    /// Class info will also includes any StaticInstance attributes the class may have.
    /// </summary>
    private static ClassInfo? GetPipeline1ClassInfo(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        AttributeData attr = ctx.Attributes.First();

        string instanceName       = attr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Name)).Value.Value as string
                                    ?? Const.DefaultSingletonPropertyName;
        InstanceType instanceType = SIHelper.ConvertToInstanceType(attr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Type)).Value.Value);
        bool @internal            = attr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Internal)).Value.Value is true;
        Constructor constructor   = SIHelper.ConvertToConstructor(attr.NamedArguments.FirstOrDefault(a => a.Key == nameof(SingletonAttribute.Constructor)).Value.Value);

        // Suppress the generated constructor if the user already declared any instance constructor.
        bool addConstructor = constructor switch
        {
            Constructor.Auto => !classSymbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared),
            Constructor.On => !classSymbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared && c.Parameters.IsEmpty),
            _ => false,
        };

        if (GetClassMetadata(classSymbol) is not (string namespaceName, string typeKind, List<string> containingTypes, string typeParameters, string hintName))
        {
            return null;
        }

        // Build unified list: [Singleton] entry first, then [StaticInstance] entries from constructors
        var usedNames       = new HashSet<string>(StringComparer.Ordinal) { instanceName };
        var staticInstances = new List<IInstanceInfo>
        {
            new SingletonInfo(instanceName, instanceType, @internal, addConstructor, string.Empty),
        };

        CollectStaticInstances(classSymbol, staticInstances, usedNames);
        return new ClassInfo(classSymbol, typeKind, typeParameters, namespaceName, containingTypes, hintName, staticInstances);
    }

    /// <summary>
    /// Handles classes that have no Singleton attribute but have StaticInstance attribute(s).
    /// </summary>
    private static ClassInfo? GetPipeline2ClassInfo(GeneratorAttributeSyntaxContext ctx)
    {
        INamedTypeSymbol? classSymbol = (ctx.TargetSymbol as IMethodSymbol)?.ContainingType;

        if (classSymbol is null)
        {
            return null;
        }

        // If the class also has [Singleton], let the singleton pipeline handle it
        if (classSymbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == Const.SingletonAttributeFullName))
        {
            return null;
        }

        // If not the first StaticInstance in class then skip
        if (GetFirstStaticInstance(classSymbol) != ctx.Attributes[0])
        {
            return null;
        }

        var usedNames       = new HashSet<string>(StringComparer.Ordinal);
        var staticInstances = new List<IInstanceInfo>();
        CollectStaticInstances(classSymbol, staticInstances, usedNames);

        return staticInstances.Count == 0
            || GetClassMetadata(classSymbol) is not (string @namespace, string typeKind, List<string> containingTypes, string typeParameters, string hintName)
            ? null
            : new ClassInfo(classSymbol, typeKind, typeParameters, @namespace, containingTypes, hintName, staticInstances);
    }

    /// <summary>
    /// Gets the first StaticInstance attribute in class.
    /// </summary>
    private static AttributeData? GetFirstStaticInstance(INamedTypeSymbol classSymbol)
    {
        foreach (IMethodSymbol ctor in classSymbol.InstanceConstructors)
        {
            foreach (AttributeData siAttr in ctor.GetAttributes())
            {
                if (siAttr.AttributeClass?.ToDisplayString() != Const.StaticInstanceAttributeFullName)
                {
                    continue;
                }

                return siAttr;
            }
        }

        return null;
    }

    /// <summary>Extracts namespace, containing-type chain, type parameters and hint-name from a class symbol.</summary>
    private static (string Namespace, string TypeKind, List<string> ContainingTypes, string TypeParameters, string HintName)? GetClassMetadata(INamedTypeSymbol classSymbol)
    {
        string namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : classSymbol.ContainingNamespace.ToDisplayString();

        string? typeKind = SIHelper.GetSymbolTypeDefinition(classSymbol);
        if (typeKind is null)
        {
            return null;
        }

        var containingTypes = new List<string>();
        INamedTypeSymbol? parent = classSymbol.ContainingType;
        while (parent is not null)
        {
            string parentTypeParams = parent.TypeParameters.Length > 0
                ? "<" + string.Join(", ", parent.TypeParameters.Select(tp => tp.Name)) + ">"
                : string.Empty;

            string? parentKind = SIHelper.GetSymbolTypeDefinition(parent);
            if (parentKind is null)
            {
                return null;
            }

            containingTypes.Add($"{parentKind} {parent.Name}{parentTypeParams}");
            parent = parent.ContainingType;
        }

        containingTypes.Reverse();

        string typeParameters = classSymbol.TypeParameters.Length > 0
            ? "<" + string.Join(", ", classSymbol.TypeParameters.Select(tp => tp.Name)) + ">"
            : string.Empty;

        string hintName = classSymbol.ToDisplayString()
            .Replace('<', '[').Replace('>', ']') // guard against generics
            .Replace(" ", string.Empty);

        return (namespaceName, typeKind, containingTypes, typeParameters, hintName);
    }

    /// <summary>
    /// Appends a <see cref="StaticInstanceInfo"/> for every <see cref="StaticInstanceAttribute"/> found on any instance constructor of <paramref name="classSymbol"/>.
    /// </summary>
    private static void CollectStaticInstances(INamedTypeSymbol classSymbol, List<IInstanceInfo> staticInstances, HashSet<string> usedNames)
    {
        foreach (IMethodSymbol ctor in classSymbol.InstanceConstructors)
        {
            ImmutableArray<AttributeData> attributes = ctor.GetAttributes();
            foreach (AttributeData siAttr in attributes)
            {
                if (siAttr.AttributeClass?.ToDisplayString() != Const.StaticInstanceAttributeFullName)
                {
                    continue;
                }

                InstanceType siType = SIHelper.ConvertToInstanceType(siAttr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Type)).Value.Value);
                bool siInternal     = siAttr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Internal)).Value.Value is true;

                if (siAttr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Name)).Value.Value is not string siName
                    || siName.IsWhiteSpace()
                    || !usedNames.Add(siName))
                {
                    continue;
                }

                string siCtorArgs = string.Empty;
                if (siAttr.ConstructorArguments.Length >= 1)
                {
                    TypedConstant argsConstant = siAttr.ConstructorArguments[0];
                    if (!argsConstant.IsNull && argsConstant.Kind == TypedConstantKind.Array && !argsConstant.Values.IsDefaultOrEmpty)
                    {
                        string[] parts = new string[argsConstant.Values.Length];
                        for (int i = 0; i < argsConstant.Values.Length; i++)
                        {
                            ITypeSymbol? paramType2 = i < ctor.Parameters.Length ? ctor.Parameters[i].Type : null;
                            parts[i] = SIHelper.FormatTypedConstant(argsConstant.Values[i], paramType2);
                        }

                        siCtorArgs = string.Join(", ", parts);
                    }
                }

                staticInstances.Add(new StaticInstanceInfo(siName, siType, siInternal, siCtorArgs));
            }
        }
    }

    private static void Execute(SourceProductionContext spc, ClassInfo info)
    {
        var sb = csb.CreateCSharpBuilder();
        _ = sb.WriteLine("// <auto-generated/>")
              .WriteLine("#nullable enable")
              .WriteLine();

        if (!string.IsNullOrEmpty(info.Namespace))
        {
            _ = sb.WriteLine($"namespace {info.Namespace}")
                  .WriteLine("{");
        }

        foreach (string containingType in info.ContainingTypes)
        {
            _ = sb.WriteLine($"partial {containingType}")
                  .WriteLine("{");
        }

        _ = sb.WriteLine($"partial {info.TypeKind} {info.FullName}")
              .WriteLine("{");

        // Private constructor is only ever needed for the singleton entry
        bool emitPrivateCtor = info.Instances.Any(si => si.AddConstructor);
        _ = sb.WriteLineIf(emitPrivateCtor, () => $$"""private {{info.ClassName}} () { }{{csb.BlankLinePost}}""");

        bool anyWeakThreadSafe = info.Instances.Any(si => si.InstanceType is InstanceType.Weak);
        string? weakLockFieldName = null;
        if (anyWeakThreadSafe)
        {
            weakLockFieldName = SIHelper.GetUniqueFieldName(info.ClassSymbol, "_weakLock");
            _ = sb.WriteLine($$"""
                             #if NET9_0_OR_GREATER
                             private static readonly Lock {{weakLockFieldName}} = new();
                             #else
                             private static readonly object {{weakLockFieldName}} = new();
                             #endif
                             """)
                  .WriteLine(string.Empty);
        }

        foreach (IInstanceInfo si in info.Instances)
        {
            string access    = si.Internal ? "internal" : "public";
            string fieldName = SIHelper.GetUniqueFieldName(info.ClassSymbol, $"_{char.ToLowerInvariant(si.InstanceName[0])}{si.InstanceName[1..]}");
            string ctorCall  = string.IsNullOrEmpty(si.ConstructorArgs) ? "new()" : $"new({si.ConstructorArgs})";

            _ = si.InstanceType switch
            {
                InstanceType.Weak => sb.WriteLine($"private static readonly WeakReference<{info.FullName}> {fieldName} = new(null!);")
                                       .WriteLine(string.Empty)
                                       .WriteLine($$"""
                                             {{access}} static {{info.FullName}} {{si.InstanceName}}
                                             {
                                                 get
                                                 {
                                                     if ({{fieldName}}.TryGetTarget(out {{info.FullName}}? currentInstance))
                                                     {
                                                         return currentInstance;
                                                     }
                                       
                                                     lock({{weakLockFieldName}})
                                                     {
                                                         if ({{fieldName}}.TryGetTarget(out currentInstance))
                                                         {
                                                             return currentInstance;
                                                         }
                                             
                                                         currentInstance = {{ctorCall}};
                                                         {{fieldName}}.SetTarget(currentInstance);
                                                         return currentInstance;
                                                     }
                                                 }
                                             }
                                             """),

                InstanceType.WeakLockless => sb.WriteLine($"private static readonly WeakReference<{info.FullName}> {fieldName} = new(null!);")
                                               .WriteLine(string.Empty)
                                               .WriteLine($$"""
                                                    {{access}} static {{info.FullName}} {{si.InstanceName}}
                                                    {
                                                        get
                                                        {
                                                            if ({{fieldName}}.TryGetTarget(out {{info.FullName}}? currentInstance))
                                                            {
                                                                return currentInstance;
                                                            }
                                               
                                                            currentInstance = {{ctorCall}};
                                                            {{fieldName}}.SetTarget(currentInstance);
                                                            return currentInstance;
                                                        }
                                                    }
                                                    """),

                InstanceType.Field => sb.WriteLine($"{access} static readonly {info.FullName} {si.InstanceName} = {ctorCall};"),

                InstanceType.Lazy => sb.WriteLine($"private static readonly Lazy<{info.FullName}> {fieldName} = new(() => {ctorCall});")
                                       .WriteLine($"{access} static {info.FullName} {si.InstanceName} => {fieldName}.Value;"),

                _ => sb.WriteLine($"{access} static {info.FullName} {si.InstanceName} {{ get; }} = {ctorCall};"),
            };
        }

        _ = sb.WriteLine("}");

        for (int i = info.ContainingTypes.Count - 1; i >= 0; i--)
        {
            _ = sb.WriteLine("}");
        }

        if (!string.IsNullOrEmpty(info.Namespace))
        {
            _ = sb.WriteLine("}");
        }

        spc.AddSource($"{info.HintName}.Singleton.g.cs", sb.ToString());
    }
}
