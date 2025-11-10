using DataDrivenConstants.Generators;
using Microsoft.CodeAnalysis.Testing;
using GenTest = DataDrivenConstants.Test.Verifiers.SourceGeneratorTestWithMarkers<DataDrivenConstants.Generators.JsonDataGenerator>;

namespace DataDrivenConstants.Test.Generators;

public class JsonDataGeneratorTest
{
    [Fact]
    public async Task NoMembersReportsDiagnostic()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class {|#0:MakeMeSomeMagicConstants|} {}
            """;

        GenTest test = new()
        {
            TestCode = source,
            TestState =
            {
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(Diagnostics.NoMembersFound).WithLocation(0)
                }
            }
        };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SimplePathCanGenerate()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string T1 = "T1";
                    public const string T2 = "T2";
                }
            }

            """;

        GenTest test = new()
        {
            TestCode = source,
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources/list.json", "[ \"T1\", \"T2\" ]")
                },
                GeneratedSources =
                {
                    (typeof(JsonDataGenerator), "MakeMeSomeMagicConstants.g.cs", gen)
                }
            }
        };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NormalizedFilePathCanGenerate()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string T1 = "T1";
                    public const string T2 = "T2";
                }
            }

            """;

        await new GenTest
        {
            TestCode = source,
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources\\list.json", "[ \"T1\", \"T2\" ]")
                },
                GeneratedSources =
                {
                    (typeof(JsonDataGenerator), "MakeMeSomeMagicConstants.g.cs", gen)
                }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MultiplePathsMatchCanGenerate()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json", "**/otherlist.json")]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string T1 = "T1";
                    public const string T2 = "T2";
                    public const string T3 = "T3";
                    public const string T4 = "T4";
                }
            }

            """;

        await new GenTest
        {
            TestCode = source,
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources/list.json", "[ \"T1\", \"T2\" ]"),
                    ("Resources/nested/list.json", "[ \"T3\"] "),
                    ("Resources/nested/otherlist.json", "[ \"T4\"] "),
                    ("Resources/nested/nomatch.json", "[ \"T4\"] ")
                },
                GeneratedSources =
                {
                    (typeof(JsonDataGenerator), "MakeMeSomeMagicConstants.g.cs", gen)
                }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task KeyPathCanGenerate()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$.*~", "**/dict.json")]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string k1 = "k1";
                    public const string k2 = "k2";
                }
            }

            """;

        await new GenTest
        {
            TestCode = source,
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources/dict.json", "{ \"k1\": \"v1\", \"k2\": \"v2\" }")
                },
                GeneratedSources =
                {
                    (typeof(JsonDataGenerator), "MakeMeSomeMagicConstants.g.cs", gen)
                }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GeneratedSymbolNamesAreValid()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string _2Number = "2Number";
                    public const string Cliffs_01_right4 = "Cliffs_01[right4]";
                    public const string Lever_Queen_s_Garden_Stag = "Lever-Queen's_Garden_Stag";
                    public const string Nailmasters_Oro_Mato = "Nailmasters_Oro_&_Mato";
                    public const string Thing_With_Backslashes = "Thing_\\With\\_Backslashes";
                }
            }

            """;

        await new GenTest
        {
            TestCode = source,
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources/list.json", """
                    [ 
                        "Lever-Queen's_Garden_Stag", 
                        "Nailmasters_Oro_&_Mato", 
                        "Cliffs_01[right4]",
                        "Thing_\\With\\_Backslashes",
                        "2Number"
                    ]
                    """)
                },
                GeneratedSources =
                {
                    (typeof(JsonDataGenerator), "MakeMeSomeMagicConstants.g.cs", gen)
                }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GeneratesWithCustomReplacement()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.ReplacementRule("-", "__")]
            [DataDrivenConstants.Marker.ReplacementRule("'", "")]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string Lever__Queens_Garden_Stag = "Lever-Queen's_Garden_Stag";
                }
            }

            """;

        await new GenTest
        {
            TestCode = source,
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources/list.json", """
                    [
                        "Lever-Queen's_Garden_Stag",
                    ]
                    """)
                },
                GeneratedSources =
                {
                    (typeof(JsonDataGenerator), "MakeMeSomeMagicConstants.g.cs", gen)
                }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GeneratesWithCustomRegexReplacement()
    {
        string source = /*lang=c#-test*/ """
            using DataDrivenConstants.Marker;

            namespace Test;

            [JsonData("$[*]", "**/list.json")]
            [ReplacementRule(@"((.)\2)(.)", "$3", ReplacementKind.Regex)]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string Clis_01_right4 = "Cliffs_01[right4]";
                    public const string Lever_Qun_s_Garden_Stag = "Lever-Queen's_Garden_Stag";
                }
            }

            """;

        await new GenTest
        {
            TestCode = source,
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources/list.json", """
                    [
                        "Cliffs_01[right4]",
                        "Lever-Queen's_Garden_Stag"
                    ]
                    """)
                },
                GeneratedSources =
                {
                    (typeof(JsonDataGenerator), "MakeMeSomeMagicConstants.g.cs", gen)
                }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }
}
