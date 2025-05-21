using JsonParser.Core;

namespace JsonParser.Tests;

public class TokenizerTests
{
    [Theory]
    [InlineData(".012")]
    [InlineData("-01")]
    [InlineData("00")]
    [InlineData("-00")]
    public void Advance_InvalidNumber_WouldBreak(string js)
    {
        Assert.ThrowsAny<Exception>(() => Utility.Parse(js));
    }

    [Theory]
    [InlineData("\"1234")]
    [InlineData("\"\\uabcd\\\"")]
    public void Advance_UnterminatedString_WouldBreak(string js)
    {
        Assert.ThrowsAny<TokenizerException>(() => Utility.Parse(js));
    }

    [Theory]
    [InlineData("\\b", "\b")]
    [InlineData("\\n", "\n")]
    [InlineData("\\\"", "\"")]
    [InlineData("\\\\", @"\")]
    [InlineData("\\/", "/")]
    [InlineData("\\u0042", "B")]
    public void Advance_EscapeSequence_ParsedRight(string js, string txt)
    {
        var actual = Utility.NextToke<string>($"\"{js}\"");
        Assert.Equal(txt, actual.Value);
    }

    [Fact]
    public void Advance_StringWithControlChar_Breaks()
    {
        var js = "\"a\nb\n\n\nc\"";
        var ex = Assert.Throws<TokenizerException>(() => Utility.NextToke(js));
        Assert.Contains("Encountered an unexpected value", ex.Message);
        Assert.Contains(" while trying to parse a string starting at (1, 1)", ex.Message);
    }
}
