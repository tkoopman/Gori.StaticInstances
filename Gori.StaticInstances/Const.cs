using Gori.CompressedStatics;

namespace Gori.StaticInstances;

#pragma warning disable SA1600 // Elements should be documented
[TarCompress]
internal static partial class Const
{
    public const string Namespace = "Gori.StaticInstances";
    public const string HintSuffix = "StaticInstances";
    public const string DefaultSingletonPropertyName = SingletonAttribute.DefaultSingletonPropertyName;
    public const string SingletonAttributeName = "SingletonAttribute";
    public const string SingletonAttributeFullName = $"{Namespace}.{SingletonAttributeName}";
    public const string StaticInstanceAttributeName = "StaticInstanceAttribute";
    public const string StaticInstanceAttributeFullName = $"{Namespace}.{StaticInstanceAttributeName}";
    public const string DiagnosticDescriptorCategory = Namespace;
    public const string DiagnosticDescriptorIDPrefix = "GSSI";

    /// <summary>
    /// Gets code for CompilerShims.cs.
    /// </summary>
    [CompressString(
        """
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
        """,
        Transformer = "CSharpRewriter",
        TransformerArguments = [CSharpRewriterOptions.Basic])]
    public static string CompilerShimsCode { get; }

    /// <summary>
    /// Gets code from <see cref="Constructor"/>, with Embedded attribute added.
    /// </summary>
    [CompressFile("Constructor.cs", Transformer = "CSharpRewriter")]
    public static string ConstructorCode { get; }

    /// <summary>
    /// Gets code from <see cref="InstanceType"/>, with Embedded attribute added.
    /// </summary>
    [CompressFile("InstanceType.cs", Transformer = "CSharpRewriter")]
    public static string InstanceTypeCode { get; }

    /// <summary>
    /// Gets code from <see cref="SingletonAttribute"/>, with Embedded attribute added.
    /// </summary>
    [CompressFile("SingletonAttribute.cs", Transformer = "CSharpRewriter")]
    public static string SingletonAttributeCode { get; }

    /// <summary>
    /// Gets code from <see cref="StaticInstanceAttribute"/>, with Embedded attribute added.
    /// </summary>
    [CompressFile("StaticInstanceAttribute.cs", Transformer = "CSharpRewriter")]
    public static string StaticInstanceAttributeCode { get; }
}
