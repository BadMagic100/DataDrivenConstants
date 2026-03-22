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
                    new DiagnosticResult(Diagnostics.NoMembersFound)
                        .WithLocation(0)
                        .WithArguments("MakeMeSomeMagicConstants")
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
    public async Task SimplePathNestedNamespaceCanGenerate()
    {
        string source = /*lang=c#-test*/ """
            namespace Test.Nested;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test.Nested
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
    public async Task MultipleAttributesCanGenerate()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json", "**/otherlist.json")]
            [DataDrivenConstants.Marker.JsonData("$.key", "**/dict.json")]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string T1 = "T1";
                    public const string T11 = "T11";
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
                    ("Resources/nested/dict.json", "{ \"key\": \"T11\" }")
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
    public async Task GeneratesWithCustomReplacementAndPascalCase()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.ReplacementRule("-", "__")]
            [DataDrivenConstants.Marker.ReplacementRule("'", "")]
            [DataDrivenConstants.Marker.NameGenerationStyle(DataDrivenConstants.Marker.NameStyle.PascalCase)]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string LeverQueensGardenStag = "Lever-Queen's_Garden_Stag";
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
    public async Task GeneratesWithCustomReplacementAndCamelCase()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.ReplacementRule("-", "__")]
            [DataDrivenConstants.Marker.ReplacementRule("'", "")]
            [DataDrivenConstants.Marker.NameGenerationStyle(DataDrivenConstants.Marker.NameStyle.CamelCase)]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string leverQueensGardenStag = "Lever-Queen's_Garden_Stag";
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
    public async Task GeneratesWithCustomReplacementAndSnakeCase()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.ReplacementRule("-", "__")]
            [DataDrivenConstants.Marker.ReplacementRule("'", "")]
            [DataDrivenConstants.Marker.NameGenerationStyle(DataDrivenConstants.Marker.NameStyle.SnakeCase)]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string lever_queens_garden_stag = "Lever-Queen's_Garden_Stag";
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
    public async Task GeneratesWithCustomReplacementAndUpperSnakeCase()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.ReplacementRule("-", "__")]
            [DataDrivenConstants.Marker.ReplacementRule("'", "")]
            [DataDrivenConstants.Marker.NameGenerationStyle(DataDrivenConstants.Marker.NameStyle.UpperSnakeCase)]
            public static partial class MakeMeSomeMagicConstants {}
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public const string LEVER_QUEENS_GARDEN_STAG = "Lever-Queen's_Garden_Stag";
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

    [Fact]
    public async Task GeneratedSymbolNamesAreValidWithInjection()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static string MakePlural([DataDrivenConstants.Marker.DataInject] string value)
                {
                    return value + "s";
                }
            }
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public static string Get_2Number() => MakePlural("2Number");
                    public static string GetCliffs_01_right4() => MakePlural("Cliffs_01[right4]");
                    public static string GetLever_Queen_s_Garden_Stag() => MakePlural("Lever-Queen's_Garden_Stag");
                    public static string GetNailmasters_Oro_Mato() => MakePlural("Nailmasters_Oro_&_Mato");
                    public static string GetThing_With_Backslashes() => MakePlural("Thing_\\With\\_Backslashes");
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
    public async Task GeneratedSymbolNamesAreValidWithInjectionAndPascalCase()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.NameGenerationStyle(DataDrivenConstants.Marker.NameStyle.PascalCase)]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static string MakePlural([DataDrivenConstants.Marker.DataInject] string value)
                {
                    return value + "s";
                }
            }
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public static string Get2Number() => MakePlural("2Number");
                    public static string GetCliffs01Right4() => MakePlural("Cliffs_01[right4]");
                    public static string GetLeverQueenSGardenStag() => MakePlural("Lever-Queen's_Garden_Stag");
                    public static string GetNailmastersOroMato() => MakePlural("Nailmasters_Oro_&_Mato");
                    public static string GetThingWithBackslashes() => MakePlural("Thing_\\With\\_Backslashes");
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
    public async Task GeneratedSymbolNamesAreValidWithInjectionAndCamelCase()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.NameGenerationStyle(DataDrivenConstants.Marker.NameStyle.CamelCase)]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static string MakePlural([DataDrivenConstants.Marker.DataInject] string value)
                {
                    return value + "s";
                }
            }
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public static string get2Number() => MakePlural("2Number");
                    public static string getCliffs01Right4() => MakePlural("Cliffs_01[right4]");
                    public static string getLeverQueenSGardenStag() => MakePlural("Lever-Queen's_Garden_Stag");
                    public static string getNailmastersOroMato() => MakePlural("Nailmasters_Oro_&_Mato");
                    public static string getThingWithBackslashes() => MakePlural("Thing_\\With\\_Backslashes");
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
    public async Task GeneratedSymbolNamesAreValidWithInjectionAndSnakeCase()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.NameGenerationStyle(DataDrivenConstants.Marker.NameStyle.SnakeCase)]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static string MakePlural([DataDrivenConstants.Marker.DataInject] string value)
                {
                    return value + "s";
                }
            }
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public static string get_2_number() => MakePlural("2Number");
                    public static string get_cliffs_01_right4() => MakePlural("Cliffs_01[right4]");
                    public static string get_lever_queen_s_garden_stag() => MakePlural("Lever-Queen's_Garden_Stag");
                    public static string get_nailmasters_oro_mato() => MakePlural("Nailmasters_Oro_&_Mato");
                    public static string get_thing_with_backslashes() => MakePlural("Thing_\\With\\_Backslashes");
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
    public async Task GeneratedSymbolNamesAreValidWithInjectionAndUpperSnakeCase()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            [DataDrivenConstants.Marker.NameGenerationStyle(DataDrivenConstants.Marker.NameStyle.UpperSnakeCase)]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static string MakePlural([DataDrivenConstants.Marker.DataInject] string value)
                {
                    return value + "s";
                }
            }
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public static string GET_2_NUMBER() => MakePlural("2Number");
                    public static string GET_CLIFFS_01_RIGHT4() => MakePlural("Cliffs_01[right4]");
                    public static string GET_LEVER_QUEEN_S_GARDEN_STAG() => MakePlural("Lever-Queen's_Garden_Stag");
                    public static string GET_NAILMASTERS_ORO_MATO() => MakePlural("Nailmasters_Oro_&_Mato");
                    public static string GET_THING_WITH_BACKSLASHES() => MakePlural("Thing_\\With\\_Backslashes");
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
    public async Task InjectionWithLastArgGeneratesCorrectly()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            public class FormatString(string Format, params string[] Args) { } 

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static FormatString MakeFormat(string format, [DataDrivenConstants.Marker.DataInject] string val)
                {
                    return new FormatString(format, val);
                }
            }
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public static global::Test.FormatString GetT1(string format) => MakeFormat(format, "T1");
                    public static global::Test.FormatString GetT2(string format) => MakeFormat(format, "T2");
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
    public async Task InjectionWithFirstArgGeneratesCorrectly()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            public class FormatString(string Format, params string[] Args) { } 

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static FormatString MakeFormat([DataDrivenConstants.Marker.DataInject] string val, string format)
                {
                    return new FormatString(format, val);
                }
            }
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public static global::Test.FormatString GetT1(string format) => MakeFormat("T1", format);
                    public static global::Test.FormatString GetT2(string format) => MakeFormat("T2", format);
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
    public async Task InjectionWithMiddleArgGeneratesCorrectly()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            public class FormatString(string Format, params string[] Args) { } 

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static FormatString MakeFormat(string format, [DataDrivenConstants.Marker.DataInject] string val, string val2)
                {
                    return new FormatString(format, val, val2);
                }
            }
            """;

        string gen = /*lang=c#-test*/ """
            namespace Test
            {
                public static partial class MakeMeSomeMagicConstants
                {
                    public static global::Test.FormatString GetT1(string format, string val2) => MakeFormat(format, "T1", val2);
                    public static global::Test.FormatString GetT2(string format, string val2) => MakeFormat(format, "T2", val2);
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
    public async Task InjectionOnNonStringArgEmitsDiagnostic()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static string MakePlural([DataDrivenConstants.Marker.DataInject] int {|#0:val|})
                {
                    return val + "s";
                }
            }
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
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(Diagnostics.DataInjectOnNonStringParameter)
                        .WithLocation(0)
                }
            }
        };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InjectionOnMultipleArgsOfSameMethodEmitsDiagnostics()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static string MakePlural([DataDrivenConstants.Marker.DataInject] string {|#0:val|}, [DataDrivenConstants.Marker.DataInject] string {|#1:val2|})
                {
                    return val + val2 + "s";
                }
            }
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
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(Diagnostics.MultipleDataInjectParameters)
                        .WithLocation(0)
                        .WithArguments("MakeMeSomeMagicConstants"),
                    new DiagnosticResult(Diagnostics.MultipleDataInjectParameters)
                        .WithLocation(1)
                        .WithArguments("MakeMeSomeMagicConstants")
                }
            }
        };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InjectionOnMultipleMethodsEmitsDiagnostics()
    {
        string source = /*lang=c#-test*/ """
            namespace Test;

            [DataDrivenConstants.Marker.JsonData("$[*]", "**/list.json")]
            public static partial class MakeMeSomeMagicConstants 
            {
                private static string MakePlural([DataDrivenConstants.Marker.DataInject] string {|#0:val|})
                {
                    return val + "s";
                }
            
                private static string MakePlural2([DataDrivenConstants.Marker.DataInject] string {|#1:val|})
                {
                    return val + "s";
                }
            }
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
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(Diagnostics.MultipleDataInjectParameters)
                        .WithLocation(0)
                        .WithArguments("MakeMeSomeMagicConstants"),
                    new DiagnosticResult(Diagnostics.MultipleDataInjectParameters)
                        .WithLocation(1)
                        .WithArguments("MakeMeSomeMagicConstants")
                }
            }
        };
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
