using System.Collections;

using Gori.Common.Roslyn.Test;
using Gori.StaticInstances.Analyzers;

using Microsoft.CodeAnalysis;

namespace Gori.StaticInstances.Testing.TestData;

public class Analyzer_GSSI001_TestData : IEnumerable<TheoryDataRow<MockSourceData>>
{
    public static readonly DiagnosticDescriptor Target = DiagnosticDescriptors.NonPartialType;
    public static readonly HashSet<string> Category = [$"Analyzer\\{Target.Id}"];
    public static readonly DiagnosticExpectation NotTargetId = new DiagnosticExpectation()
    {
        IsExpected = false,
        Id = Target.Id,
    };

    public IEnumerator<TheoryDataRow<MockSourceData>> GetEnumerator()
    {
        // -------------------------------------------------------------------------
        // Both attributes checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public class MyService 
            {
                private MyService() { }

                [StaticIntance(1, Name = "Default")]
                public MyService(int value) { }
            }
            """,

            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 55,
                    AdditionalPositions = [],
                    Message = "MyService",
                    CodeFixExpectations =
                    [
                        new CodeFixExpectation()
                        {
                            IsExpected = true,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public partial class MyService",
                        },
                        new CodeFixExpectation()
                        {
                            IsExpected = false,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public class MyService",
                        },
                    ],
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on non-partial class with both [Singleton] and [StaticInstance]",
        };

        // -------------------------------------------------------------------------
        // Singleton checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public class MyService { }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 55,
                    AdditionalPositions = [],
                    Message = "MyService",
                    CodeFixExpectations =
                    [
                        new CodeFixExpectation()
                        {
                            IsExpected = true,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public partial class MyService",
                        },
                        new CodeFixExpectation()
                        {
                            IsExpected = false,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public class MyService",
                        },
                    ],
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on non-partial class with [Singleton]",
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            [Singleton]
            public partial class MyService { }
            """,
            Expectations =
            [
                NotTargetId
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on partial class with [Singleton]",
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public class Outer
            {
                [Singleton]
                public partial class Inner { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 42,
                    AdditionalPositions = [],
                    Message = "Outer",
                    CodeFixExpectations =
                    [
                        new CodeFixExpectation()
                        {
                            IsExpected = true,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public partial class Outer",
                        },
                        new CodeFixExpectation()
                        {
                            IsExpected = false,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public class Outer",
                        },
                    ],
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on non-partial outer class with, nested partial class with [Singleton]",
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public class Outer
            {
                [Singleton]
                public class Inner { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 42,
                    AdditionalPositions = [],
                    Message = "Outer",
                    CodeFixExpectations =
                    [
                        new CodeFixExpectation()
                        {
                            IsExpected = true,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public partial class Outer",
                        },
                        new CodeFixExpectation()
                        {
                            IsExpected = false,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public class Outer",
                        },
                    ],
                },
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 86,
                    AdditionalPositions = [],
                    Message = "Inner",
                    CodeFixExpectations =
                    [
                        new CodeFixExpectation()
                        {
                            IsExpected = true,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public partial class Inner",
                        },
                        new CodeFixExpectation()
                        {
                            IsExpected = false,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public class Inner",
                        },
                    ],
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on non-partial outer and inner classes when, nested class has [Singleton]"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
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
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on partial outer or inner classes when, nested class has [Singleton]"
        };

        // -------------------------------------------------------------------------
        // StaticInstance checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public class MyService 
            {
                [StaticInstance(Name = "Instance")]
                public MyService() { }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 42,
                    AdditionalPositions = [],
                    Message = "MyService",
                    CodeFixExpectations =
                    [
                        new CodeFixExpectation()
                        {
                            IsExpected = true,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public partial class MyService",
                        },
                        new CodeFixExpectation()
                        {
                            IsExpected = false,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public class MyService",
                        },
                    ],
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on non-partial class with [StaticInstance]"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public partial class MyService 
            {
                [StaticInstance(Name = "Instance")]
                public MyService() { }
            }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on partial class with [StaticInstance]"
        };

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            using Gori.StaticInstances;
            public class Outer
            {
                public partial class Inner 
                {
                    [StaticInstance(Name = "Instance")]
                    public Inner() { }
                }
            }
            """,
            Expectations =
            [
                new DiagnosticExpectation(Target)
                {
                    IsExpected = true,
                    Position = 42,
                    AdditionalPositions = [],
                    Message = "Outer",
                    CodeFixExpectations =
                    [
                        new CodeFixExpectation()
                        {
                            IsExpected = true,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public partial class Outer",
                        },
                        new CodeFixExpectation()
                        {
                            IsExpected = false,
                            EquivalenceKey = "MakeTypePartial",
                            Code = "public class Outer",
                        },
                    ],
                },
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Raised on non-partial outer class with, nested partial class with [StaticInstance]"
        };

        // -------------------------------------------------------------------------
        // No attribute checks
        // -------------------------------------------------------------------------

        yield return new TheoryDataRow<MockSourceData>(new MockSourceData()
        {
            SourceCode =
            """
            public class MyService { }
            """,
            Expectations =
            [
                NotTargetId,
            ],
        })
        {
            Traits = { { "Category", Category } },
            Label = $"{Target.Id}: Not raised on class when no attributes used"
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
