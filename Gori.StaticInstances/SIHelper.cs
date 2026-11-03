using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Gori.StaticInstances;

/// <summary>
/// Static methods used by static instance code generator and analyzers.
/// </summary>
internal static class SIHelper
{
    /// <summary>
    /// Converts object read from TypedConstant's value to InstanceType.
    /// </summary>
    /// <param name="value">Value assigned.</param>
    /// <returns>Value assigned converted to InstanceType or InstanceType.Default.</returns>
    public static InstanceType ConvertToInstanceType(object? value)
        => value switch
        {
            0 => InstanceType.Default,
            1 => InstanceType.Field,
            2 => InstanceType.Lazy,
            3 => InstanceType.Weak,
            4 => InstanceType.WeakLockless,
            InstanceType type => type,
            _ => InstanceType.Default,
        };

    /// <summary>
    /// Converts object read from TypedConstant's value to Constructor.
    /// </summary>
    /// <param name="value">Value assigned.</param>
    /// <returns>Value assigned converted to Constructor or Constructor.Auto.</returns>
    public static Constructor ConvertToConstructor(object? value)
        => value switch
        {
            0 => Constructor.Auto,
            1 => Constructor.On,
            2 => Constructor.Off,
            Constructor con => con,
            _ => Constructor.Auto,
        };

    /// <summary>
    /// Outputs null/default with casting so that if multiple constructors
    /// with same number of parameters the correct one is still used.
    /// </summary>
    /// <param name="paramType">Parameter's type to get null value for.</param>
    /// <returns>Appropriate null or default value for the type.</returns>
    public static string GetNullValue(ITypeSymbol? paramType)
        => paramType is not null
            ? paramType.IsValueType
                ? $"({paramType.ToDisplayString()})default"
                : $"({paramType.ToDisplayString()})null"
            : "default";

    /// <summary>
    /// Converts symbol's TypeKind into the C# syntax for it. (class, struct, record, record struct)
    /// Null if TypeKind is unsupported.
    /// </summary>
    /// <param name="symbol">A class, struct or record symbol.</param>
    /// <returns>Source code keyword(s) for the symbol.</returns>
    public static string? GetSymbolTypeDefinition(INamedTypeSymbol symbol)
        => symbol.IsRecord
            ? symbol.TypeKind switch { TypeKind.Class => "record", TypeKind.Struct => "record struct", _ => null, }
            : symbol.TypeKind switch { TypeKind.Class => "class", TypeKind.Struct => "struct", _ => null, };

    /// <summary>
    /// Allocates a field name, adding a numeric suffix when the name is already reserved.
    /// </summary>
    /// <param name="allocatedNames">Existing and generated member names, updated with the allocated name.</param>
    /// <param name="baseFieldName">Field name you want to get unique version of.</param>
    /// <returns><paramref name="baseFieldName"/> with incremented numeric suffix added if needed to make unique.</returns>
    public static string GetUniqueFieldName(HashSet<string> allocatedNames, string baseFieldName)
    {
        string candidateName = baseFieldName;
        int suffix = 1;
        while (!allocatedNames.Add(candidateName))
        {
            candidateName = $"{baseFieldName}{suffix++}";
        }

        return candidateName;
    }

    /// <summary>
    /// Search the current class and inherited classes, for any named symbol members matching the name.
    /// </summary>
    /// <param name="classSymbol">Class, struct or record to check for field's existence in.</param>
    /// <param name="fieldName">Field name you want to look for.</param>
    /// <returns><see langword="true"/> if field does exist in the class; else <see langword="false"/>.</returns>
    public static bool ClassContainsName(INamedTypeSymbol classSymbol, string fieldName)
    {
        INamedTypeSymbol? currentClass = classSymbol;

        bool first = true;
        while (currentClass != null)
        {
            bool hasConflict = first
                ? currentClass.GetMembers()
                              .Any(f => f.Name == fieldName)
                : currentClass.GetMembers()
                              .Any(f => f.DeclaredAccessibility is not Accessibility.Private && f.Name == fieldName);

            first = false;
            if (hasConflict)
            {
                return true;
            }

            // Move up the inheritance tree
            currentClass = currentClass.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Renders a <see cref="TypedConstant"/> as a C# literal suitable for use in generated code.
    /// </summary>
    /// <param name="constant">The typed constant to render as c# literal code.</param>
    /// <param name="paramType">The type expected to be rendered as. Will try and convert if different from <paramref name="constant"/>.</param>
    /// <returns>C# literal representation of <paramref name="constant"/>'s value.</returns>
    public static string FormatTypedConstant(TypedConstant constant, ITypeSymbol? paramType = null)
    {
        if (constant.IsNull || constant.Value is null)
        {
            return GetNullValue(paramType);
        }

        switch (constant.Kind)
        {
            case TypedConstantKind.Primitive:
                // When the constant's type doesn't match the constructor parameter type
                // (because params object[] boxes everything), coerce to the target type.
                if (paramType is not null && constant.Type?.SpecialType != paramType.SpecialType)
                {
                    string? coerced = paramType.SpecialType switch
                    {
                        SpecialType.System_Single   => $"{Convert.ToSingle(constant.Value, CultureInfo.InvariantCulture):R}f",
                        SpecialType.System_Double   => $"{Convert.ToDouble(constant.Value, CultureInfo.InvariantCulture):R}d",
                        SpecialType.System_Decimal  => $"{Convert.ToDecimal(constant.Value, CultureInfo.InvariantCulture)}m",
                        SpecialType.System_Int64    => $"{Convert.ToInt64(constant.Value, CultureInfo.InvariantCulture)}L",
                        SpecialType.System_UInt64   => $"{Convert.ToUInt64(constant.Value, CultureInfo.InvariantCulture)}UL",
                        SpecialType.System_UInt32   => $"{Convert.ToUInt32(constant.Value, CultureInfo.InvariantCulture)}u",
                        SpecialType.System_Int32    => $"{Convert.ToInt32(constant.Value, CultureInfo.InvariantCulture)}",
                        SpecialType.System_Int16    => $"(short){Convert.ToInt16(constant.Value, CultureInfo.InvariantCulture)}",
                        SpecialType.System_UInt16   => $"(ushort){Convert.ToUInt16(constant.Value, CultureInfo.InvariantCulture)}",
                        SpecialType.System_Byte     => $"(byte){Convert.ToByte(constant.Value, CultureInfo.InvariantCulture)}",
                        SpecialType.System_SByte    => $"(sbyte){Convert.ToSByte(constant.Value, CultureInfo.InvariantCulture)}",
                        SpecialType.System_String   => "\"" + SymbolDisplay.FormatLiteral($"{constant.Value}", false) + "\"",
                        _ => null,
                    };

                    if (coerced is not null)
                    {
                        return coerced;
                    }
                }

                return constant.Value switch
                {
                    string s => "\"" + SymbolDisplay.FormatLiteral(s, false) + "\"",
                    bool b => b ? "true" : "false",
                    char c => $"'{SymbolDisplay.FormatLiteral(c, false)}'",
                    float f => $"{f:R}f",
                    double d => $"{d:R}d",
                    decimal m => $"{m}m",
                    _ => constant.Value.ToString() ?? GetNullValue(paramType),
                };

            case TypedConstantKind.Enum:
                return "(" + constant.Type!.ToDisplayString() + ")" + constant.Value;

            case TypedConstantKind.Type:
                return "typeof(" + ((ITypeSymbol)constant.Value!).ToDisplayString() + ")";

            case TypedConstantKind.Array:
                if (constant.Values.IsDefaultOrEmpty)
                {
                    return "new object[0]";
                }

                return "new object[] { " + string.Join(", ", constant.Values.Select(v => FormatTypedConstant(v))) + " }";

            case TypedConstantKind.Error:
            default:
                return constant.Value.ToString() ?? GetNullValue(paramType);
        }
    }
}
