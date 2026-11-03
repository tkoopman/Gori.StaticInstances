using System.Collections;

using Gori.Common.Roslyn.Test;

namespace Gori.StaticInstances.Testing.TestData;

public class CombinedGenerator_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public const string TestPrefix = "Combined";

    public const string Category = "Generator\\Combined";

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
                Content = "public static MyService MyInstance { get; } = new MyService();",
            },
        ];

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Default behavior, no options
        // -------------------------------------------------------------------------

        string prefix = $"{TestPrefix} Default Behavior";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            [method: StaticInstance(Name = "MyInstance")]
            public partial class MyService () { }
            """,
            Expectations = DefaultExpectations[..2],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class primary constructor (StaticInstance not applied)",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            [method: StaticInstance(Name = "MyInstance")]
            public partial class MyService ()
            {
                static MyService() { }
            }
            """,
            Expectations = DefaultExpectations[..2],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Class with primary constructor and static constructor (StaticInstance not applied)",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService
            {
                [StaticInstance(Name = "MyInstance")]
                private MyService () { }
            }
            """,
            Expectations = DefaultExpectations,
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
                [StaticInstance(Name = "MyInstance")]
                public MyService (int value) { }
            }
            """,
            Expectations = DefaultExpectations,
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
                    Content = "public static MyService<T> Instance { get; } = new MyService<T>();",
                },
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
            namespace My.App
            {
                [Singleton]
                [method: StaticInstance(Name = "MyInstance")]
                public partial class MyService () { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "My.App.MyService.StaticInstances.g.cs",
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
                public partial class Inner
                {
                    [StaticInstance(Name = "MyInstance")]
                    public Inner() { }
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
                    Content = "public static Inner Instance { get; } = new Inner();",
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
            public partial class GrandParent
            {
                public partial class Parent
                {
                    [Singleton]
                    public partial class Child
                    {
                        [method: StaticInstance(Name = "MyInstance")]
                        public Child() { }
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
                    Content = "public static Child Instance { get; } = new Child();",
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

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class Outer<T>
            {
                [Singleton]
                public partial class Inner
                {
                    [StaticInstance(Name = "MyInstance")]
                    public Inner() { }
                }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner.StaticInstances.g.cs",
                    Content = "public static Inner Instance { get; } = new Inner();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner.StaticInstances.g.cs",
                    Content = "public static Inner MyInstance { get; } = new Inner();",
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
                public partial class Inner<T>
                {
                    [StaticInstance(Name = "MyInstance")]
                    public Inner() { }
                }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer+Inner`1.StaticInstances.g.cs",
                    Content = "public static Inner<T> Instance { get; } = new Inner<T>();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer+Inner`1.StaticInstances.g.cs",
                    Content = "public static Inner<T> MyInstance { get; } = new Inner<T>();",
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
                public partial class Inner<TValue>
                {
                    [StaticInstance(Name = "MyInstance")]
                    public Inner() { }
                }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner`1.StaticInstances.g.cs",
                    Content = "public static Inner<TValue> Instance { get; } = new Inner<TValue>();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "Outer`1+Inner`1.StaticInstances.g.cs",
                    Content = "public static Inner<TValue> MyInstance { get; } = new Inner<TValue>();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Generic class nested inside generic class",
        };

        //// -------------------------------------------------------------------------
        //// Single option behaviors - Type
        //// -------------------------------------------------------------------------

        prefix = $"{TestPrefix} Type";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.Property)]
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
            [Singleton(Type = InstanceType.Weak)]
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
                    Content = "private static readonly global::System.Threading.Lock _instanceWeakLock = new global::System.Threading.Lock();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly object _instanceWeakLock = new object();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly global::System.WeakReference<MyService> _instance = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly global::System.Threading.Lock _myInstanceWeakLock = new global::System.Threading.Lock();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly object _myInstanceWeakLock = new object();",
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
                    Content = "public static MyService MyInstance",
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
            [Singleton(Type = InstanceType.WeakLockless)]
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
                    Content = "private static readonly global::System.WeakReference<MyService> _instance = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "_instance.SetTarget(currentInstance);",
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
                    Content = "public static MyService MyInstance",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "_myInstance.SetTarget(currentInstance);",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "Lock",
                    ContentCriteria = MatchCriteria.Contains | MatchCriteria.CaseInsensitive,
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix} = WeakLockless: Basic class",
        };

        //// -------------------------------------------------------------------------
        //// Mixture of different options together
        //// -------------------------------------------------------------------------

        prefix = $"{TestPrefix} Mixed Bag";

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.WeakLockless)]
            public partial class MyService
            {
                [StaticInstance(Name = "MyInstance1", Type = InstanceType.Property)]
                public MyService () { }
                [StaticInstance(2, Name = "MyInstance2", Type = InstanceType.Weak)]
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
                    Content = "private static readonly global::System.WeakReference<MyService> _instance = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService MyInstance1 { get; } = new MyService();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly global::System.Threading.Lock _myInstance2WeakLock = new global::System.Threading.Lock();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "private static readonly global::System.WeakReference<MyService> _myInstance2 = new global::System.WeakReference<MyService>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService.StaticInstances.g.cs",
                    Content = "public static MyService MyInstance2",
                },
            ],
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
            public partial class GrandParent
            {
                public partial class Parent
                {
                    [StaticInstance(Name = "MyInstance")]
                    public Parent () { }

                    [Singleton]
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
                    HintName = "GrandParent.StaticInstances.g.cs",
                    Content = "public static GrandParent Instance { get; } = new GrandParent();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent.StaticInstances.g.cs",
                    Content = "public static Parent MyInstance { get; } = new Parent();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent+Child.StaticInstances.g.cs",
                    Content = "public static Child Instance { get; } = new Child();",
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

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService1 { }

            [Singleton]
            public partial class MyService2 () { }

            public partial class MyService3
            {
                [StaticInstance(Name = "MyInstance")]
                public MyService3() { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService1.StaticInstances.g.cs",
                    Content = "public static MyService1 Instance { get; } = new MyService1();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService2.StaticInstances.g.cs",
                    Content = "public static MyService2 Instance { get; } = new MyService2();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyService3.StaticInstances.g.cs",
                    Content = "public static MyService3 MyInstance { get; } = new MyService3();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class (multiple in single file)",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton(Type = InstanceType.Lazy)]
            public partial class GrandParent
            {
                public class ParentBase 
                {
                    private void _myInstance() { }
                }
                public partial class Parent : ParentBase
                {
                    private string _myInstance { get; } = null!;
                    [StaticInstance(Name = "MyInstance", Type = InstanceType.Weak)]
                    public Parent () { }
                    [Singleton]
                    public partial class Child : ParentBase
                    {
                        [StaticInstance(Name = "MyInstance", Type = InstanceType.WeakLockless)]
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
                    HintName = "GrandParent.StaticInstances.g.cs",
                    Content = "private static readonly global::System.Lazy<GrandParent> _instance = new global::System.Lazy<GrandParent>(() => new GrandParent());",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent.StaticInstances.g.cs",
                    Content = "public static GrandParent Instance => _instance.Value;",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent.StaticInstances.g.cs",
                    Content = "private static readonly global::System.WeakReference<Parent> _myInstance1 = new global::System.WeakReference<Parent>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent.StaticInstances.g.cs",
                    Content = "private static readonly global::System.WeakReference<Parent> _myInstance1 = new global::System.WeakReference<Parent>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent.StaticInstances.g.cs",
                    Content = "public static Parent MyInstance",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent+Child.StaticInstances.g.cs",
                    Content = "public static Child Instance { get; } = new Child();",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent+Child.StaticInstances.g.cs",
                    Content = "private static readonly global::System.WeakReference<Child> _myInstance = new global::System.WeakReference<Child>(null!);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "GrandParent+Parent+Child.StaticInstances.g.cs",
                    Content = "public static Child MyInstance",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Basic class deeply nested inside basic classes, with field name conflicts",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace MyApp;
            [Singleton]
            public partial class MyService
            {
                private MyService () { }
            
                [StaticInstance("localhost", 5432, Name = "Dev")]
                private MyService (string host, int port) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "public static MyService Dev { get; } = new MyService(\"localhost\", 5432);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Constructor parameters",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace MyApp;
            [Singleton]
            public partial class MyService
            {
                private MyService () { }
            
                [StaticInstance("localhost", 5432, Name = "Dev")]
                [StaticInstance("staging-db", 5432, Name = "Staging")]
                private MyService (string host, int port) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "public static MyService Dev { get; } = new MyService(\"localhost\", 5432);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "public static MyService Staging { get; } = new MyService(\"staging-db\", 5432);",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Multiple instances on single constructor",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace MyApp;
            [Singleton]
            public partial class MyService
            {
                private MyService () { }
            
                [StaticInstance("One", Name = "Dev")]
                [StaticInstance("Two", Name = "Dev")]
                private MyService (string host) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "public static MyService Dev { get; } = new MyService(\"One\");",
                },
                new FileExpectation()
                {
                    IsExpected = false,
                    Content = "Two",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Duplicate names silently skipped",
        };

        yield return new(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            namespace MyApp;
            [Singleton]
            public partial class MyService
            {
                private MyService () { }
            
                [StaticInstance("host", Name = "Instance")]
                private MyService (string host) { }
            }
            """,
            Expectations =
            [
                FileExpectation.NoOthers,
                new FileExpectation()
                {
                    IsExpected = false,
                    Content = "host",
                },
                new FileExpectation()
                {
                    IsExpected = true,
                    HintName = "MyApp.MyService.StaticInstances.g.cs",
                    Content = "public static MyService Instance { get; } = new MyService();",
                },
            ],
        })
        {
            Traits = { { "Category", [Category] } },
            Label = $"{prefix}: Duplicate names with Singleton",
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
