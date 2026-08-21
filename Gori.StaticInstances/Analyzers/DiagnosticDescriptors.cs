using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Analyzers;

/// <summary>
/// All diagnostic descriptors for this analyzer.
/// </summary>
internal static class DiagnosticDescriptors
{
    /// <summary>
    /// <see cref="SingletonAttribute"/> on non-partial type.
    /// </summary>
    public static readonly DiagnosticDescriptor NonPartialType = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}001",
    title: "Non-partial type",
    messageFormat: "Type '{0}' is not declared as partial",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "Types with source-generation attributes must be declared as partial so that the generator can extend them.");

    /// <summary>
    /// <see cref="SingletonAttribute"/> on a type that has explicit constructors but none is parameterless.
    /// </summary>
    public static readonly DiagnosticDescriptor NoDefaultConstructor = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}002",
    title: "Missing parameterless constructor",
    messageFormat: "Type '{0}' has explicit constructors but no parameterless constructor; the generated instance property requires one",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "When the type declares any constructor the generator cannot add its own. A parameterless constructor must exist so the generated instance property can call new().");

    /// <summary>
    /// <see cref="SingletonAttribute"/> type has an explicit parameterless constructor that is not private or protected.
    /// </summary>
    public static readonly DiagnosticDescriptor NonPrivateDefaultConstructor = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}003",
    title: "Non-private parameterless constructor",
    messageFormat: "Type '{0}' has a non-private parameterless constructor; consider making it private or protected to prevent direct instantiation",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true,
    description: "A type with a non-private parameterless constructor can be instantiated directly, undermining the singleton pattern.");

    /// <summary>
    /// <see cref="StaticInstanceAttribute.Name"/> is not a valid C# identifier.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidInstanceName = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}004",
    title: "Name is not a valid C# identifier",
    messageFormat: "'{0}' is not a valid C# identifier; the Name property must be a valid non-keyword identifier",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "The Name property is used as the generated property name and must therefore be a valid, non-keyword C# identifier.");

    /// <summary>
    /// Two or more instance names on the same type collide (including the name used by <see cref="SingletonAttribute"/> and any <see cref="StaticInstanceAttribute"/>).
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateInstanceName = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}005",
    title: "Duplicate static instance name",
    messageFormat: "Instance name '{0}' is used more than once on type '{1}'; all [Singleton] and [StaticInstance] names on a type must be unique",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "Each generated static property must have a unique name. Check the Name property of [Singleton] and all [StaticInstance] attributes on the type.");

    /// <summary>
    /// Two or more <see cref="StaticInstanceAttribute"/> definitions on the same constructor (or a <see cref="SingletonAttribute"/> combined with a no-arg <see cref="StaticInstanceAttribute"/>) share identical constructor arguments and will therefore create equal objects.
    /// </summary>
    public static readonly DiagnosticDescriptor IdenticalInstances = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}006",
    title: "Identical static instances",
    messageFormat: "Constructor on '{0}' has multiple [StaticInstance] or [Singleton] definitions with identical constructor arguments; these will always produce equal objects",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description: "When two or more static instance definitions on the same constructor supply identical arguments, each generated property will independently create an object equal to the others. Consider removing the redundant definitions.");

    /// <summary>
    /// A <see cref="StaticInstanceAttribute"/> supplies a different number of constructor arguments
    /// than the constructor it is placed on requires.
    /// </summary>
    public static readonly DiagnosticDescriptor ArgumentCountMismatch = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}007",
    title: "Constructor argument count mismatch",
    messageFormat: "[StaticInstance(\"{0}\")] supplies {1} argument(s) but constructor '{2}' has {3} parameter(s)",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "The number of arguments passed to [StaticInstance] must match the number of parameters of the constructor it is placed on.");

    /// <summary>
    /// A <see cref="StaticInstanceAttribute"/> argument type does not match the corresponding constructor parameter type.
    /// The generator will attempt to convert the value, but this may not match the user's intent.
    /// </summary>
    public static readonly DiagnosticDescriptor ArgumentTypeMismatch = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}008",
    title: "Constructor argument type mismatch",
    messageFormat: "[StaticInstance(\"{0}\")] argument {1} has type '{2}' but parameter '{3}' expects type '{4}'; the generator will attempt to convert the value",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description: "The type of a value passed to [StaticInstance] does not match the corresponding constructor parameter type. The generator will attempt to coerce the value, but verify the generated output is correct.");

    /// <summary>
    /// A <see cref="StaticInstanceAttribute"/> argument type does not match the corresponding constructor parameter type.
    /// The generator will attempt to convert the value, but this may not match the user's intent.
    /// </summary>
    public static readonly DiagnosticDescriptor ValueArgumentIsNull = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}009",
    title: "Constructor value argument is set to null",
    messageFormat: "[StaticInstance(\"{0}\")] argument {1} is null but parameter '{2}' is a value type; the generator will set to '({3})default'",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true,
    description: "The null value passed to [StaticInstance] matches a value type parameter. The generator will set it to default.");

    /// <summary>
    /// <see cref="StaticInstanceAttribute"/> applied to a primary constructor via <c>[method: StaticInstance]</c>,
    /// which is not supported. Not supported due to Roslyn issue #80511.
    /// </summary>
    /// <remarks>
    /// Technically will work if Singleton attribute also used on class (Pipeline 1),
    /// but raising error anyway, for consistency and would also raise GSSI003 and GSSI006 anyway.
    /// </remarks>
    public static readonly DiagnosticDescriptor StaticInstanceOnPrimaryConstructor = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}010",
    title: "Unsupported primary constructor usage",
    messageFormat: "[StaticInstance] on the primary constructor of '{0}' via [method: StaticInstance] is not supported; place [StaticInstance] on an explicit constructor instead",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "Due to Roslyn issue #80511, the [method: StaticInstance] attribute-target specifier on a primary constructor is not supported by the generator. Declare an explicit constructor and place [StaticInstance] directly on it.");

    /// <summary>
    /// The C# language version is too low for the target framework.
    /// </summary>
    public static readonly DiagnosticDescriptor LanguageVersionTooLow = new(
    id: $"{Const.DiagnosticDescriptorIDPrefix}011",
    title: "C# language version too low",
    messageFormat: "C# language version '{0}' is too low; {1} or higher is required for the current target framework",
    category: Const.DiagnosticDescriptorCategory,
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "The source generator requires at least C# 11.0, or C# 13.0 when targeting .NET 9 or higher. Increase the LangVersion property in your project file.");
}
