namespace Gori.StaticInstances;

#pragma warning disable SA1600 // Elements should be documented
internal static class Const
{
    public const string Namespace = "Gori.StaticInstances";
    public const string HintSuffix = "StaticInstances";
    public const string DefaultSingletonPropertyName = "Instance";
    public const string SingletonAttributeShortName = "Singleton";
    public const string SingletonAttributeName = "SingletonAttribute";
    public const string SingletonAttributeFullName = $"{Namespace}.{SingletonAttributeName}";
    public const string StaticInstanceAttributeShortName = "StaticInstance";
    public const string StaticInstanceAttributeName = "StaticInstanceAttribute";
    public const string StaticInstanceAttributeFullName = $"{Namespace}.{StaticInstanceAttributeName}";
    public const string DiagnosticDescriptorCategory = Namespace;
    public const string DiagnosticDescriptorIDPrefix = "GSSI";
    public const string EmbeddedAttribute = "[global::Microsoft.CodeAnalysis.Embedded]";
    public const string AttributesHintName = $"{Namespace}.Attributes.g.cs";

    /// <summary>
    /// Code copied from CompilerShims.cs.
    /// </summary>
    public const string CompilerShimsCode = """
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
        """;

    /// <summary>
    /// Code copied from <see cref="Constructor"/>, with Embedded attribute added.
    /// </summary>
    public const string ConstructorCode = $$"""
        /// <summary>
        /// Defines if Singleton attribute should generate
        /// a private default constructor if a default constructor doesn't already exist.
        /// </summary>
        {{EmbeddedAttribute}}
        internal enum Constructor
        {
            /// <summary>
            /// If no explicitly defined constructors exist,
            /// will add private default constructor.
            /// </summary>
            Auto,

            /// <summary>
            /// Will add private default constructor,
            /// unless a default constructor already exist.
            /// </summary>
            On,

            /// <summary>
            /// Will never add a constructor.
            /// </summary>
            Off,
        }
        """;

    /// <summary>
    /// Code copied from <see cref="InstanceType"/>, with Embedded attribute added.
    /// </summary>
    public const string InstanceTypeCode = $$"""
        /// <summary>
        /// Change how static instance will be generated.
        /// </summary>
        {{EmbeddedAttribute}}
        internal enum InstanceType
        {
            /// <summary>
            /// Instance will be created as a property.
            /// </summary>
            Default,

            /// <summary>
            /// Instance will be created as a property.
            /// </summary>
            Property = Default,

            /// <summary>
            /// Instance will be created as a field.
            /// </summary>
            Field,

            /// <summary>
            /// Property backed by <see cref="global::System.Lazy{T}"/>.
            /// </summary>
            Lazy,

            /// <summary>
            /// Property backed by <see cref="global::System.WeakReference{T}"/> with locking when needing to initialize new instance.
            /// </summary>
            Weak,

            /// <summary>
            /// Property backed by <see cref="global::System.WeakReference{T}"/>. Non-Thread-Safe version of <see cref="Weak"/>.
            /// </summary>
            WeakLockless,
        }
        """;

    /// <summary>
    /// Code copied from <see cref="SingletonAttribute"/>,
    /// with Embedded attribute added, and
    /// Name default value changed from Const.DefaultSingletonPropertyName to "{{DefaultSingletonPropertyName}}" (including quotes).
    /// </summary>
    public const string SingletonAttributeCode = $$"""      
        /// <summary>
        /// Attribute to make class implement the singleton pattern.
        /// </summary>
        [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        {{EmbeddedAttribute}}
        internal sealed class SingletonAttribute : global::System.Attribute
        {
            /// <summary>
            /// Gets or sets the name that should be assigned to the static instance property.
            /// Must be a valid C# identifier that is not a reserved or contextual keyword.
            /// </summary>
            public string Name { get; set; } = "{{DefaultSingletonPropertyName}}";

            /// <summary>
            /// Gets or sets the type of static instance to generate.
            /// </summary>
            public InstanceType Type { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the accessibility modifier given to the static
            /// instance property should be internal, instead of the default public.
            /// </summary>
            public bool Internal { get; set; }

            /// <summary>
            /// Gets or sets when Singleton attribute should add a private default constructor.
            /// By default creates a private default constructor, if no other constructors exist.
            /// </summary>
            public Constructor Constructor { get; set; }
        }
        """;

    /// <summary>
    /// Code copied from <see cref="StaticInstanceAttribute"/>, with Embedded attribute added.
    /// </summary>
    public const string StaticInstanceAttributeCode = $$"""
        /// <summary>
        /// Attribute placed on a constructor to generate a named static property that creates
        /// an instance of the class using that constructor with the supplied arguments.
        /// Multiple attributes can be placed on the same constructor to produce several
        /// named properties. All names must be unique within the class, including the name
        /// used by <see cref="SingletonAttribute"/>.
        /// </summary>
        [global::System.AttributeUsage(global::System.AttributeTargets.Constructor, AllowMultiple = true, Inherited = false)]
        {{EmbeddedAttribute}}
        internal sealed class StaticInstanceAttribute : global::System.Attribute
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StaticInstanceAttribute"/> class.
            /// </summary>
            /// <param name="args">The arguments to pass to the constructor.</param>
            public StaticInstanceAttribute(params object[] args)
            {
            }

            /// <summary>
            /// Gets or sets the name that should be assigned to the static instance property.
            /// Must be a valid C# identifier that is not a reserved or contextual keyword.
            /// </summary>
            public required string Name { get; set; }

            /// <summary>
            /// Gets or sets the type of static instance to generate.
            /// </summary>
            public InstanceType Type { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the accessibility modifier given to the static
            /// instance property should be internal, instead of the default public.
            /// </summary>
            public bool Internal { get; set; }
        }
        """;
}
