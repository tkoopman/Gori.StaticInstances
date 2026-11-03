using System.Collections;

using Gori.Common.Roslyn.Test;

namespace Gori.StaticInstances.Testing.TestData;

public class StaticInstanceGenerator_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public const string TestPrefix = "StaticInstance";

    public const string Category = "Generator\\StaticInstance";

    public static readonly KeyedExpectation[] DefaultExpectations =
        [
            FileExpectation.NoOthers,
            new FileExpectation()
            {
                IsExpected = true,
                HintName = "MyService.StaticInstances.g.cs",
                Content = "public static MyService MyInstance { get; } = new MyService();",
            },
            new FileExpectation()
            {
                IsExpected = false,
                Content = "private MyService ()",
            },
        ];

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {

        // -------------------------------------------------------------------------
        // All options used
        // -------------------------------------------------------------------------

        string prefix = $"{TestPrefix} All Options";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService 
            {
                [StaticInstance(Name = "MyInstance", Type = InstanceType.WeakLockless, Internal = true)]
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
                    Content = "private static readonly global::System.WeakReference<MyService> _myInstance = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content =
                    """
                    internal static MyService MyInstance
                    {
                        get
                        {
                            if (_myInstance.TryGetTarget(out MyService? currentInstance))
                            {
                                return currentInstance;
                            }
                            currentInstance = new MyService();
                            _myInstance.SetTarget(currentInstance);
                            return currentInstance;
                        }
                    }
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
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
            public partial class MyService 
            {
                [StaticInstance(Name = "MyInstance", Type = InstanceType.Weak, Internal = true)]
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
                    Content = "private static readonly global::System.WeakReference<MyService> _myInstance = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content =
                    """
                    internal static MyService MyInstance
                    {
                        get
                        {
                            if (_myInstance.TryGetTarget(out MyService? currentInstance))
                            {
                                return currentInstance;
                            }

                            lock(_myInstanceWeakLock)
                            {
                                if (_myInstance.TryGetTarget(out currentInstance))
                                {
                                    return currentInstance;
                                }

                                currentInstance = new MyService();
                                _myInstance.SetTarget(currentInstance);
                                return currentInstance;
                            }
                        }
                    }
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: All non-defaults except ThreadSafe",
        };

        // -------------------------------------------------------------------------
        // Default behavior, no options
        // -------------------------------------------------------------------------

        prefix = $"{TestPrefix} Default Behavior";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            public partial class MyService 
            {
                [Gori.StaticInstances.StaticInstance(Name = "MyInstance")]
                public MyService () { }
            }
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
            [method: StaticInstance(Name = "MyInstance")]
            public partial class MyService () { }
            """,
            Expectations = [FileExpectation.NoOthers],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class primary constructor - Fails to generate code due to Roslyn issue #80511",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(100, Name = "MyInstance")]
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
                    Content = "public static MyService MyInstance { get; } = new MyService(100);",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class, constructor with parameters",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService<T>
            {
                [StaticInstance(Name = "MyInstance")]
                public MyService () { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService`1.StaticInstances.g.cs",
                    Content = "public static MyService<T> MyInstance { get; } = new MyService<T>();",
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
            public partial class GrandParent
            {
                public partial class Parent
                {
                    public partial class Child 
                    {
                        [StaticInstance(Name = "MyInstance")]
                        public Child () { }
                    }
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
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent+Child.StaticInstances.g.cs",
                    Content = "public static Child MyInstance { get; } = new Child();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class deeply nested inside basic classes",
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
            public partial class MyService 
            {
                [StaticInstance(Name = "MyInstance", Internal = false)]
                public MyService () { }
            }
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
            public partial class MyService 
            {
                [StaticInstance(Name = "MyInstance", Internal = true)]
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
            public partial class MyService 
            {
                [StaticInstance(Name = "MyInstance", Type = InstanceType.Property)]
                public MyService () { }
            }
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
            public partial class MyService 
            {
                [StaticInstance(Name = "MyInstance", Type = InstanceType.Weak)]
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
                    Content =
                    """
                    #if NET9_0_OR_GREATER
                        private static readonly global::System.Threading.Lock _myInstanceWeakLock = new global::System.Threading.Lock();
                    #else
                        private static readonly object _myInstanceWeakLock = new object();
                    #endif
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
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
                    Content =
                    """
                    public static MyService MyInstance
                    {
                        get
                        {
                            if (_myInstance.TryGetTarget(out MyService? currentInstance))
                            {
                                return currentInstance;
                            }

                            lock(_myInstanceWeakLock)
                            {
                                if (_myInstance.TryGetTarget(out currentInstance))
                                {
                                    return currentInstance;
                                }

                                currentInstance = new MyService();
                                _myInstance.SetTarget(currentInstance);
                                return currentInstance;
                            }
                        }
                    }
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
                },
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
            public partial class MyService 
            {
                [StaticInstance(Name = "MyInstance", Type = InstanceType.WeakLockless)]
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
                    Content = "private static readonly global::System.WeakReference<MyService> _myInstance = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content =
                    """
                    public static MyService MyInstance
                    {
                        get
                        {
                            if (_myInstance.TryGetTarget(out MyService? currentInstance))
                            {
                                return currentInstance;
                            }
                    
                            currentInstance = new MyService();
                            _myInstance.SetTarget(currentInstance);
                            return currentInstance;
                        }
                    }
                    """,
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.SimplifyWhiteSpace,
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = WeakLockless: Basic class",
        };

        prefix = $"{TestPrefix} Bonus Tests";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace My.App
            {
                public partial class MyService 
                {
                    [StaticInstance(Name = "MyInstance")]
                    public MyService () { }
                }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "namespace My.App",
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
            Label = $"{prefix}: Basic class in namespace",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class Outer
            {
                public partial class Inner 
                {
                    [StaticInstance(Name = "MyInstance")]
                    public Inner () { }
                }
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
                    Content = "public static Inner MyInstance { get; } = new Inner();",
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
            public partial class MyService
            {
                [StaticInstance([default], Name = "Default")]
                private MyService (int value) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Default { get; } = new MyService((int)default);",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Constructor using default as parameter value",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance(1.0f, Name = "Default1a")]
                [StaticInstance(1, Name = "Default1b")]
                private MyService (int value) { }
            
                [StaticInstance(1, Name = "Default2")]
                private MyService(float value) { }
            
                [StaticInstance(1, Name = "Default3")]
                private MyService(string value) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Default1a { get; } = new MyService(1);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Default1b { get; } = new MyService(1);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Default2 { get; } = new MyService(1f);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Default3 { get; } = new MyService(\"1\");",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Constructors - multiple similar",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace MyApp;
            public partial class MyService
            {
                [StaticInstance("line1\nline2", Name = "Multi")]
                private MyService (string value) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = @"MyService(""line1\nline2"")",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "line1\nline2",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Generates Escaped String Literals",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace MyApp;
            public partial class MyService
            {
                [StaticInstance(null, Name = "Null")]
                private MyService () { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "MyService()",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    Content = "null)",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Null params array",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace MyApp;
            public partial class MyService
            {
                [StaticInstance([null], Name = "Null")]
                private MyService (object? obj) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "MyService((object?)null)",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Null param",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService
            {
                [StaticInstance("first", Name = "Duplicate")]
                private MyService(string value) { }

                [StaticInstance(2, Name = "Duplicate")]
                private MyService(int value) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                DiagnosticExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Duplicate { get; } = new MyService(\"first\");",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "new MyService(2)",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{TestPrefix}: Duplicate name on separate constructors does not fail generator",
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
