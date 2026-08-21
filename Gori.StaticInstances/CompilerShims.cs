#pragma warning disable IDE0001, IDE0130, SA1402, SA1403, SA1600, SA1649
// Polyfill types that enable the 'required' keyword on netstandard2.0.
// The C# compiler only requires these types to exist; it never calls them at runtime.

/*
 * Following code should also be copied into Const.CompilerShimsCode
 */

#if !NET7_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [global::System.AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = false)]
    internal sealed class RequiredMemberAttribute : global::System.Attribute
    {
    }

    [global::System.AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : global::System.Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
            => FeatureName = featureName;

        public string FeatureName { get; }

        public bool IsOptional { get; set; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    [global::System.AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : global::System.Attribute
    {
    }
}
#endif
