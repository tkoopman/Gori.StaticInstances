using System.Collections;

using Gori.Common.Roslyn.Test;

namespace Gori.StaticInstances.Testing.TestData;

public class SingletonGenerator_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public const string TestPrefix = "Singleton";

    public const string Category = "Generator\\Singleton";

    public static readonly KeyedExpectation[] DefaultExpectations =
        [
            FileExpectation.NoOthers,
            new FileExpectation()
            {
                IsExpected = true,
                HintName = "MyService.StaticInstances.g.cs",
                Content = "public static MyService Instance { get; } = new MyService();",
            },
            new FileExpectation()
            {
                IsExpected = true,
                HintName = "MyService.StaticInstances.g.cs",
                Content = "private MyService () { }",
            },
        ];

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {

        // -------------------------------------------------------------------------
        // Single option behaviors - Name + Internal
        // -------------------------------------------------------------------------

        string prefix = $"{TestPrefix} All Options";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Name = "MyInstance", Type = InstanceType.WeakLockless, Constructor = Constructor.Off, Internal = true)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly global::System.WeakReference<MyService> _myInstance = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "internal static MyService MyInstance",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: All non-defaults",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Name = "MyInstance", Type = InstanceType.Weak, Internal = true)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "new global::System.Threading.Lock()",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly global::System.WeakReference<MyService> _myInstance = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "internal static MyService MyInstance",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: All non-defaults except Constructor and ThreadSafe",
        };

        // -------------------------------------------------------------------------
        // Default behavior, no options
        // -------------------------------------------------------------------------

        prefix = $"{TestPrefix} Default Behavior";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService { }
            """,
            Expectations = DefaultExpectations,
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                static MyService() { }
            }
            """,
            Expectations = DefaultExpectations,
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Class with static constructor",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService () { }
            """,
            Expectations = [.. DefaultExpectations[..2]],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Existing primary default constructor",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                private MyService () { }
            }
            """,
            Expectations = [.. DefaultExpectations[..2]],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Existing private default constructor",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                public MyService (int value) { }
            }
            """,
            Expectations = [.. DefaultExpectations[..2]],
        })
        {
            Traits = { { "Category", [Category, "Would Fail to Compile but Expected"] } },
            Label = $"{prefix}: Existing public constructor with parameters",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService<T> { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService`1.StaticInstances.g.cs",
                    Content = "public static MyService<T> Instance { get; } = new MyService<T>();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService`1.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Generic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace My.App
            {
                [Singleton]
                public partial class MyService { }
            }
            """,
            Expectations =
            [.. DefaultExpectations,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "namespace My.App",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class in namespace",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class Outer
            {
                [Singleton]
                public partial class Inner { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer+Inner.StaticInstances.g.cs",
                    Content =
                    """
                    partial class Outer
                    {
                        partial class Inner
                        {
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer+Inner.StaticInstances.g.cs",
                    Content = "private Inner () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer+Inner.StaticInstances.g.cs",
                    Content = "public static Inner Instance { get; } = new Inner();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class nested inside basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class GrandParent
            {
                public partial class Parent
                {
                    [Singleton]
                    public partial class Child { }
                }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent+Child.StaticInstances.g.cs",
                    Content =
                    """
                    partial class GrandParent
                    {
                        partial class Parent
                        {
                            partial class Child
                            {
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent+Child.StaticInstances.g.cs",
                    Content = "private Child () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent+Child.StaticInstances.g.cs",
                    Content = "public static Child Instance { get; } = new Child();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class deeply nested inside basic classes",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class Outer<T>
            {
                [Singleton]
                public partial class Inner { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner.StaticInstances.g.cs",
                    Content =
                    """
                    partial class Outer<T>
                    {
                        partial class Inner
                        {
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner.StaticInstances.g.cs",
                    Content = "private Inner () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner.StaticInstances.g.cs",
                    Content = "public static Inner Instance { get; } = new Inner();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class nested inside generic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class Outer
            {
                [Singleton]
                public partial class Inner<T> { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer+Inner`1.StaticInstances.g.cs",
                    Content =
                    """
                    partial class Outer
                    {
                        partial class Inner<T>
                        {
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer+Inner`1.StaticInstances.g.cs",
                    Content = "private Inner () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer+Inner`1.StaticInstances.g.cs",
                    Content = "public static Inner<T> Instance { get; } = new Inner<T>();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Generic class nested inside basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class Outer<TKey>
            {
                [Singleton]
                public partial class Inner<TValue> { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner`1.StaticInstances.g.cs",
                    Content =
                    """
                    partial class Outer<TKey>
                    {
                        partial class Inner<TValue>
                        {
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner`1.StaticInstances.g.cs",
                    Content = "private Inner () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner`1.StaticInstances.g.cs",
                    Content = "public static Inner<TValue> Instance { get; } = new Inner<TValue>();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Generic class nested inside generic class",
        };

        // -------------------------------------------------------------------------
        // Single option behaviors - Constructor
        // -------------------------------------------------------------------------

        prefix = $"{TestPrefix} Constructor";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.Auto)]
            public partial class MyService { }
            """,
            Expectations = DefaultExpectations,
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = Default (Auto): Basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.Off)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    Content = "private MyService () { }",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = Off: Basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.On)]
            public partial class MyService () { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    Content = "private MyService () { }",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = On: Basic class with primary default constructor.",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.On)]
            public partial class MyService 
            {
                public MyService () { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = On: Basic class with default constructor.",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.On)]
            public partial class MyService 
            {
                public MyService (int value) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = On: Basic class with no default constructor, but has other constructors.",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Constructor = Constructor.On)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = On: Basic class with no constructors.",
        };

        // -------------------------------------------------------------------------
        // Single option behaviors - Name
        // -------------------------------------------------------------------------

        prefix = $"{prefix} Name";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Name = "MyInstance")]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService MyInstance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class",
        };

        // -------------------------------------------------------------------------
        // Single option behaviors - Internal
        // -------------------------------------------------------------------------

        prefix = $"{TestPrefix} Internal";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Internal = false)]
            public partial class MyService { }
            """,
            Expectations = DefaultExpectations,
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = Default (false): Basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Internal = true)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "internal static MyService Instance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = true: Basic class",
        };

        // -------------------------------------------------------------------------
        // Multiple options - Name + Internal
        // -------------------------------------------------------------------------

        prefix = $"{TestPrefix} Name + Internal";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Name = "MyInstance", Internal = true)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "internal static MyService MyInstance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = true: Basic class",
        };

        // -------------------------------------------------------------------------
        // Single option behaviors - Type
        // -------------------------------------------------------------------------

        prefix = $"{TestPrefix} Type";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.Property)]
            public partial class MyService { }
            """,
            Expectations = DefaultExpectations,
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = Default (Property): Basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.Field)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static readonly MyService Instance = new MyService();",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "get;",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = Field: Basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.Lazy)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private MyService () { }",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly global::System.Lazy<MyService> _instance = new global::System.Lazy<MyService>(() => new MyService());",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance => _instance.Value;",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "get;",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = Lazy: Basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.Weak)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content =
                    """
                    // <auto-generated/>
                    #nullable enable

                    partial class MyService
                    {
                        private MyService () { }

                    #if NET9_0_OR_GREATER
                        private static readonly global::System.Threading.Lock _instanceWeakLock = new global::System.Threading.Lock();
                    #else
                        private static readonly object _instanceWeakLock = new object();
                    #endif

                        private static readonly global::System.WeakReference<MyService> _instance = new global::System.WeakReference<MyService>(null!);

                        /// <summary>
                        /// Gets the singleton instance of MyService
                        /// </summary>
                        public static MyService Instance
                        {
                            get
                            {
                                if (_instance.TryGetTarget(out MyService? currentInstance))
                                {
                                    return currentInstance;
                                }

                                lock(_instanceWeakLock)
                                {
                                    if (_instance.TryGetTarget(out currentInstance))
                                    {
                                        return currentInstance;
                                    }

                                    currentInstance = new MyService();
                                    _instance.SetTarget(currentInstance);
                                    return currentInstance;
                                }
                            }
                        }
                    }
                    """,
                    ContentCriteria = MatchCriteria.Exact | MatchCriteria.SimplifyWhiteSpace,
                }
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = Weak: Basic class",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.WeakLockless)]
            public partial class MyService { }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content =
                    """
                    // <auto-generated/>
                    #nullable enable

                    partial class MyService
                    {
                        private MyService () { }

                        private static readonly global::System.WeakReference<MyService> _instance = new global::System.WeakReference<MyService>(null!);

                        /// <summary>
                        /// Gets the singleton instance of MyService
                        /// </summary>
                        public static MyService Instance
                        {
                            get
                            {
                                if (_instance.TryGetTarget(out MyService? currentInstance))
                                {
                                    return currentInstance;
                                }

                                currentInstance = new MyService();
                                _instance.SetTarget(currentInstance);
                                return currentInstance;
                            }
                        }
                    }
                    """,
                    ContentCriteria = MatchCriteria.Exact | MatchCriteria.SimplifyWhiteSpace,
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = WeakLockless: Basic class",
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
