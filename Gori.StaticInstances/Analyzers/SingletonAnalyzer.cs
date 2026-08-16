using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gori.StaticInstances.Analyzers;

/// <summary>
/// Performs code analysis for <see cref="SingletonAttribute"/> and <see cref="StaticInstanceAttribute"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SingletonAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        DiagnosticDescriptors.NonPartialType,
        DiagnosticDescriptors.NoDefaultConstructor,
        DiagnosticDescriptors.NonPrivateDefaultConstructor,
        DiagnosticDescriptors.InvalidInstanceName,
        DiagnosticDescriptors.DuplicateInstanceName,
        DiagnosticDescriptors.IdenticalInstances,
        DiagnosticDescriptors.ArgumentCountMismatch,
        DiagnosticDescriptors.ArgumentTypeMismatch,
        DiagnosticDescriptors.ValueArgumentIsNull,
        DiagnosticDescriptors.StaticInstanceOnPrimaryConstructor,
    ];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeTypeSymbol, SymbolKind.NamedType);
        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.RecordDeclaration);
    }

    private static bool IsSingletonAttribute(INamedTypeSymbol? symbol)
        => symbol is not null
        && string.Equals(symbol.Name, nameof(SingletonAttribute), StringComparison.Ordinal)
        && string.Equals(symbol.ContainingNamespace.ToDisplayString(), Const.Namespace, StringComparison.Ordinal);

    private static bool IsStaticInstanceAttribute(INamedTypeSymbol? symbol)
        => symbol is not null
        && string.Equals(symbol.Name, nameof(StaticInstanceAttribute), StringComparison.Ordinal)
        && string.Equals(symbol.ContainingNamespace.ToDisplayString(), Const.Namespace, StringComparison.Ordinal);

    private static bool IsPartial(INamedTypeSymbol typeSymbol)
        => typeSymbol.DeclaringSyntaxReferences.All(r =>
            r.GetSyntax() is TypeDeclarationSyntax typeDecl &&
            typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static void ReportIfNoDefaultConstructor(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol, AttributeData singletonAttr)
    {
        // Only relevant when the user has written at least one constructor — the generator
        // will not add its own in that case, so a parameterless one must already exist.
        bool hasExplicitConstructors = typeSymbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared);
        if (!hasExplicitConstructors)
        {
            return;
        }

        bool hasParameterlessConstructor = typeSymbol.InstanceConstructors
            .Any(c => !c.IsImplicitlyDeclared && c.Parameters.IsEmpty);

        if (!hasParameterlessConstructor)
        {
            Location location = GetAttributeLocation(singletonAttr, typeSymbol);
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NoDefaultConstructor,
                location,
                typeSymbol.Name));
        }
    }

    private static void ReportIfDefaultConstructorNotPrivate(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol, bool generatorConstructorDisabled)
    {
        bool hasExplicitConstructors = typeSymbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared);

        IMethodSymbol? explicitParameterlessCtor = typeSymbol.InstanceConstructors
            .FirstOrDefault(c => !c.IsImplicitlyDeclared && c.Parameters.IsEmpty);

        if (explicitParameterlessCtor is not null)
        {
            // Explicit parameterless constructor — flag if it is not private or protected.
            if (explicitParameterlessCtor.DeclaredAccessibility is not (Accessibility.Private or Accessibility.Protected))
            {
                Location location = explicitParameterlessCtor.Locations.FirstOrDefault() ?? Location.None;
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NonPrivateDefaultConstructor,
                    location,
                    typeSymbol.Name));
            }
        }
        else if (generatorConstructorDisabled && !hasExplicitConstructors)
        {
            // Constructor = false with no explicit constructors: the compiler supplies an
            // implicit public constructor that the generator will not replace with a private one.
            Location location = typeSymbol.Locations.FirstOrDefault() ?? Location.None;
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NonPrivateDefaultConstructor,
                location,
                typeSymbol.Name));
        }
    }

    private static void AnalyzeTypeSymbol(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        bool hasSingleton = typeSymbol.GetAttributes().Any(a => IsSingletonAttribute(a.AttributeClass));
        bool hasStaticInstance = typeSymbol.InstanceConstructors
            .Any(ctor => ctor.GetAttributes().Any(a => IsStaticInstanceAttribute(a.AttributeClass)));

        if (!hasSingleton && !hasStaticInstance)
        {
            return;
        }

        AttributeData? singletonAttr = null;

        // Checks that only apply when [Singleton] is present
        if (hasSingleton)
        {
            singletonAttr = typeSymbol.GetAttributes().First(a => IsSingletonAttribute(a.AttributeClass));
            Constructor generatorConstructorArg = SIHelper.ConvertToConstructor(singletonAttr.NamedArguments.FirstOrDefault(a => a.Key == nameof(SingletonAttribute.Constructor)).Value.Value);

            // If the type has explicit constructors but none is parameterless, new() in the
            // generated Instance property will not compile.
            // No need to check however if Constructor equals On, as will always create.
            if (generatorConstructorArg != Constructor.On)
            {
                ReportIfNoDefaultConstructor(context, typeSymbol, singletonAttr);
            }

            // If a parameterless constructor is (or will be) non-private, direct instantiation
            // is possible and the singleton pattern can be bypassed.
            ReportIfDefaultConstructorNotPrivate(context, typeSymbol, generatorConstructorArg == Constructor.Off);

            ReportIfInvalidName(context, typeSymbol, singletonAttr);
        }

        // Checks that only apply when [StaticInstance] is present
        if (hasStaticInstance)
        {
            // Check any StaticInstances on any constructor
            foreach (IMethodSymbol ctor in typeSymbol.InstanceConstructors)
            {
                foreach (AttributeData siAttr in ctor.GetAttributes())
                {
                    if (IsStaticInstanceAttribute(siAttr.AttributeClass))
                    {
                        ReportIfInvalidName(context, typeSymbol, siAttr);
                        ReportIfArgumentMismatch(context, ctor, siAttr);
                    }
                }
            }
        }

        // Checks to run if either attribute exists
        ReportIfNotPartial(context, typeSymbol);
        ReportIfDuplicateInstanceNames(context, typeSymbol, singletonAttr);
        ReportIfIdenticalInstances(context, typeSymbol, singletonAttr);

        // Check each ancestor containing type
        INamedTypeSymbol? containingType = typeSymbol.ContainingType;
        while (containingType != null)
        {
            ReportIfNotPartial(context, containingType);
            containingType = containingType.ContainingType;
        }
    }

    /// <summary>
    /// Reports <see cref="DiagnosticDescriptors.StaticInstanceOnPrimaryConstructor"/> for any
    /// <c>[method: StaticInstance]</c> attribute lists found on a class declaration.
    /// Uses the shared <see cref="SemanticModel"/> provided by the syntax node action context.
    /// </summary>
    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        var typeSyntax = (TypeDeclarationSyntax)context.Node;

        foreach (AttributeListSyntax attrList in typeSyntax.AttributeLists)
        {
            if (attrList.Target?.Identifier.Text != "method")
            {
                continue;
            }

            foreach (AttributeSyntax attr in attrList.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attr).Symbol is IMethodSymbol attrCtor
                    && IsStaticInstanceAttribute(attrCtor.ContainingType))
                {
                    string typeName = typeSyntax.Identifier.Text;
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.StaticInstanceOnPrimaryConstructor,
                        attr.GetLocation(),
                        typeName));
                }
            }
        }
    }

    private static bool IsValidCSharpIdentifier(string name)
        => !string.IsNullOrEmpty(name)
        && SyntaxFacts.IsValidIdentifier(name)
        && !SyntaxFacts.IsReservedKeyword(SyntaxFacts.GetKeywordKind(name))
        && !SyntaxFacts.IsContextualKeyword(SyntaxFacts.GetContextualKeywordKind(name));

    private static Location GetAttributeLocation(AttributeData attr, ISymbol? parent)
        => attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? parent?.Locations.FirstOrDefault() ?? Location.None;

    private static void ReportIfInvalidName(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol, AttributeData attr)
    {
        string? name = attr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Name)).Value.Value as string;
        if (name is null or Const.DefaultSingletonPropertyName)
        {
            return; // default — always valid
        }

        if (!IsValidCSharpIdentifier(name))
        {
            Location location = GetAttributeLocation(attr, typeSymbol);
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.InvalidInstanceName,
                location,
                name));
        }
    }

    private static void ReportIfDuplicateInstanceNames(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol, AttributeData? singletonAttr)
    {
        var seen  = new HashSet<string>(StringComparer.Ordinal);

        // Seed with the singleton name if present
        if (singletonAttr is not null)
        {
            string singletonName = singletonAttr.ConstructorArguments.Length >= 1 && singletonAttr.ConstructorArguments[0].Value is string ctorName
                ? ctorName
                : singletonAttr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Name)).Value.Value as string
                  ?? Const.DefaultSingletonPropertyName;

            _ = seen.Add(singletonName);
        }

        // Collect all StaticInstance names from all instance constructors
        foreach (IMethodSymbol ctor in typeSymbol.InstanceConstructors)
        {
            foreach (AttributeData siAttr in ctor.GetAttributes())
            {
                if (!IsStaticInstanceAttribute(siAttr.AttributeClass))
                {
                    continue;
                }

                string siName = siAttr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Name)).Value.Value as string ?? string.Empty;

                if (string.IsNullOrEmpty(siName))
                {
                    continue;
                }

                if (!seen.Add(siName))
                {
                    Location location = GetAttributeLocation(siAttr, ctor);
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.DuplicateInstanceName,
                        location,
                        siName,
                        typeSymbol.Name));
                }
            }
        }
    }

    /// <summary>
    /// For each constructor, builds a string key from its [StaticInstance] arguments. Also seeds the
    /// primary/parameterless constructor with one no-arg entry for [Singleton]. If any argument key
    /// appears more than once on that constructor a GSSI006 warning is emitted.
    /// </summary>
    private static void ReportIfIdenticalInstances(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol, AttributeData? singletonAttr)
    {
        // The parameterless constructor is where the singleton entry lives.
        // It may be explicit or implicit; either way it is the constructor called
        // by the generated "Instance" property (new T()).
        IMethodSymbol? parameterlessCtor = typeSymbol.InstanceConstructors
            .FirstOrDefault(c => c.Parameters.IsEmpty);

        foreach (IMethodSymbol ctor in typeSymbol.InstanceConstructors)
        {
            var argKeyCounts = new Dictionary<string, List<Location>>(StringComparer.Ordinal);

            // Seed the singleton entry on the parameterless constructor only.
            if (singletonAttr is not null && ReferenceEquals(ctor, parameterlessCtor))
            {
                argKeyCounts[string.Empty] = [GetAttributeLocation(singletonAttr, typeSymbol)];
            }

            foreach (AttributeData siAttr in ctor.GetAttributes())
            {
                if (!IsStaticInstanceAttribute(siAttr.AttributeClass))
                {
                    continue;
                }

                // Build a key from the constructor args passed to the attribute (index 0 is now
                // the params object[] args since Name moved to a named argument).
                // Unwrap the params array elements so that an empty params array and the
                // singleton seed (string.Empty) both produce the same key.
                // Skip attributes whose arg count doesn't match: GSSI007 will be raised instead.
                string argKey;
                if (siAttr.ConstructorArguments.Length >= 1)
                {
                    TypedConstant paramsArg = siAttr.ConstructorArguments[0];
                    if (!paramsArg.IsNull && !paramsArg.Values.IsDefault && !paramsArg.Values.IsEmpty)
                    {
                        if (paramsArg.Values.Length != ctor.Parameters.Length)
                        {
                            continue; // Wrong arg count — GSSI007 handles this, skip GSSI006
                        }

                        argKey = BuildNormalisedArgKey(paramsArg.Values, ctor);
                    }
                    else
                    {
                        if (ctor.Parameters.Length != 0)
                        {
                            continue; // Wrong arg count — GSSI007 handles this, skip GSSI006
                        }

                        argKey = string.Empty;
                    }
                }
                else
                {
                    if (ctor.Parameters.Length != 0)
                    {
                        continue; // Wrong arg count — GSSI007 handles this, skip GSSI006
                    }

                    argKey = string.Empty;
                }

                if (argKeyCounts.TryGetValue(argKey, out List<Location>? ctorData))
                {
                    ctorData.Add(GetAttributeLocation(siAttr, ctor));
                }
                else
                {
                    argKeyCounts[argKey] = [GetAttributeLocation(siAttr, ctor)];
                }
            }

            foreach (List<Location> locations in argKeyCounts.Where(kvp => kvp.Value.Count > 1).Select(kvp => kvp.Value))
            {
                Location location = locations[0];
                locations.RemoveAt(0);
                if (ctor.Locations.FirstOrDefault() is Location ctorLocation)
                {
                    locations.Add(ctorLocation);
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.IdenticalInstances,
                    location,
                    locations,
                    typeSymbol.Name));
            }
        }
    }

    private static void ReportIfArgumentMismatch(SymbolAnalysisContext context, IMethodSymbol ctor, AttributeData siAttr)
    {
        int paramCount = ctor.Parameters.Length;
        string instanceName = siAttr.NamedArguments.FirstOrDefault(a => a.Key == nameof(IStaticInstanceOptions.Name)).Value.Value as string ?? string.Empty;

        TypedConstant paramsArg = siAttr.ConstructorArguments.Length >= 1 ? siAttr.ConstructorArguments[0] : default;

        // ConstructorArguments[0] is now the params object[] args (Name moved to named arg).
        // A null array (user wrote [StaticInstance(Name = "X", null)]) is treated as 0 supplied args,
        // which will trigger GSSI007 if the constructor requires parameters.
        int suppliedArgCount = paramsArg.IsNull || paramsArg.Values.IsDefault
                                ? 0
                                : paramsArg.Values.Length;

        if (suppliedArgCount != paramCount)
        {
            Location location = GetAttributeLocation(siAttr, ctor);
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.ArgumentCountMismatch,
                location,
                instanceName,
                suppliedArgCount,
                ctor.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                paramCount));

            return; // Do not want to also raise GSSI008
        }

        if (siAttr.ConstructorArguments.Length < 1)
        {
            return;
        }

        for (int i = 0; i < paramCount; i++)
        {
            ITypeSymbol? argType   = paramsArg.Values[i].Type;
            ITypeSymbol  paramType = ctor.Parameters[i].Type;

            bool argIsNull = argType is null || paramsArg.Values[i].IsNull;

            // If types match or arg is null and param is not a value type
            // then all good. We do not enforce #nullable
            if (SymbolEqualityComparer.Default.Equals(argType, paramType)
                || (argIsNull && !paramType.IsValueType))
            {
                continue;
            }

            Location location = GetAttributeLocation(siAttr, ctor);

            if (argIsNull && paramType.IsValueType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.ValueArgumentIsNull,
                    location,
                    instanceName,
                    i + 1,
                    ctor.Parameters[i].Name,
                    paramType.ToDisplayString()));

                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.ArgumentTypeMismatch,
                location,
                instanceName,
                i + 1,
                argType?.ToDisplayString() ?? "null",
                ctor.Parameters[i].Name,
                paramType.ToDisplayString()));
        }
    }

    /// <summary>
    /// Builds a comparison key for a set of attribute arg values normalised against the
    /// constructor parameter types, so that e.g. <c>1</c> (int) and <c>1.0f</c> (float)
    /// on the same <c>int</c> parameter both produce the key <c>"1"</c>.
    /// </summary>
    private static string BuildNormalisedArgKey(ImmutableArray<TypedConstant> values, IMethodSymbol ctor)
    {
        string[] parts = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            ITypeSymbol paramType = ctor.Parameters[i].Type;
            parts[i] = SIHelper.FormatTypedConstant(values[i], paramType);
        }

        return string.Join(",", parts);
    }

    private static void ReportIfNotPartial(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol)
    {
        if (!IsPartial(typeSymbol))
        {
            Location location = typeSymbol.Locations.FirstOrDefault() ?? Location.None;
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NonPartialType,
                location,
                typeSymbol.Name));
        }
    }
}
