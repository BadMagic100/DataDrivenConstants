using DataDrivenConstants.Util;

namespace DataDrivenConstants.Test.Util;

public class RenamerTest
{
    public static TheoryData<string, string[]> SeparateSymbolWordsData => new()
    {
        // empty and trivial
        { "", [] },
        { "word", ["word"] },
        { "WORD", ["WORD"] },

        // camelCase
        { "helloWorld", ["hello", "World"] },
        { "myVarName", ["my", "Var", "Name"] },

        // PascalCase
        { "HelloWorld", ["Hello", "World"] },
        { "MyClassName", ["My", "Class", "Name"] },

        // snake_case
        { "snake_case", ["snake", "case"] },
        { "three_word_name", ["three", "word", "name"] },

        // UPPER_SNAKE_CASE
        { "UPPER_SNAKE", ["UPPER", "SNAKE"] },
        { "UPPER_SNAKE_CASE", ["UPPER", "SNAKE", "CASE"] },

        // kebab-case
        { "kebab-case", ["kebab", "case"] },
        { "multi-word-kebab", ["multi", "word", "kebab"] },

        // digits are treated as lowercase, so they bind to the preceding word
        // and trigger a split on the next uppercase letter
        { "area51Report", ["area51", "Report"] },
        { "version2Update", ["version2", "Update"] },

        // consecutive uppercase characters should be wise to uppercase acronyms leading into new words
        { "HTMLParser", ["HTML", "Parser"] },
        { "parseXMLData", ["parse", "XML", "Data"] },
        { "parseHTML", ["parse", "HTML"] },

        // digits act like lowercase: they bind to the preceding word and do NOT trigger
        // the acronym lookahead split (char.IsLower(digit) == false), but they DO set
        // state to Lower, so an uppercase letter following a digit splits as normal
        { "V8Engine", ["V8", "Engine"] },
        { "MP3Player", ["MP3", "Player"] },
        { "HTML5Parser", ["HTML5", "Parser"] },
        { "Base64Encoded", ["Base64", "Encoded"] },
        { "AES256Encryption", ["AES256", "Encryption"] },

        // leading, trailing, and consecutive separators are ignored/collapsed
        { "_leadingUnderscore", ["leading", "Underscore"] },
        { "trailingUnderscore_", ["trailing", "Underscore"] },
        { "double__underscore", ["double", "underscore"] },

        // mixed styles
        { "pascal_Case_andCamel", ["pascal", "Case", "and", "Camel"] },
        { "camelCase_with_UPPER_SNAKE", ["camel", "Case", "with", "UPPER", "SNAKE"] },
        { "My_camelCase-kebab", ["My", "camel", "Case", "kebab"] },
    };

    [Theory]
    [MemberData(nameof(SeparateSymbolWordsData))]
    public void SeparateSymbolWords_ReturnsExpectedWords(string input, string[] expectedWords)
    {
        List<string> result = Renamer.SeparateSymbolWords(input);
        Assert.Equal(expectedWords, result);
    }

    // GenerateName uses [Fact] tests rather than [Theory] + [MemberData] because
    // ReplacementRule and NameStyle are internal types, which cannot appear in a public method signature.

    [Fact]
    public void GenerateName_MinimalTransform_LeavesValidNamesAlone()
        => Assert.Equal("hello_world", Renamer.GenerateName("hello_world", [], NameStyle.MinimalTransform));

    [Fact]
    public void GenerateName_MinimalTransform_SanitizesInvalidCharacters()
        => Assert.Equal("hello_world", Renamer.GenerateName("hello world", [], NameStyle.MinimalTransform));

    [Fact]
    public void GenerateName_PascalCase_BasicInput()
        => Assert.Equal("HelloWorld", Renamer.GenerateName("hello_world", [], NameStyle.PascalCase));

    [Fact]
    public void GenerateName_PascalCase_PreservesAcronyms()
        => Assert.Equal("ParseXMLData", Renamer.GenerateName("parseXMLData", [], NameStyle.PascalCase));

    [Fact]
    public void GenerateName_CamelCase_BasicInput()
        => Assert.Equal("helloWorld", Renamer.GenerateName("HelloWorld", [], NameStyle.CamelCase));

    [Fact]
    public void GenerateName_CamelCase_LowercasesLeadingAcronym()
        => Assert.Equal("htmlParser", Renamer.GenerateName("HTMLParser", [], NameStyle.CamelCase));

    [Fact]
    public void GenerateName_SnakeCase_BasicInput()
        => Assert.Equal("hello_world", Renamer.GenerateName("HelloWorld", [], NameStyle.SnakeCase));

    [Fact]
    public void GenerateName_SnakeCase_LowercasesAcronyms()
        => Assert.Equal("parse_xml_data", Renamer.GenerateName("parseXMLData", [], NameStyle.SnakeCase));

    [Fact]
    public void GenerateName_UpperSnakeCase_BasicInput()
        => Assert.Equal("HELLO_WORLD", Renamer.GenerateName("helloWorld", [], NameStyle.UpperSnakeCase));

    [Fact]
    public void GenerateName_UpperSnakeCase_PreservesAcronyms()
        => Assert.Equal("HTML_PARSER", Renamer.GenerateName("HTMLParser", [], NameStyle.UpperSnakeCase));

    [Fact]
    public void GenerateName_ReplacementsAppliedBeforeStyling()
        => Assert.Equal("ParseHTMLData", Renamer.GenerateName("parse_xml_data", [new ReplacementRule("xml", "HTML", false)], NameStyle.PascalCase));
}
