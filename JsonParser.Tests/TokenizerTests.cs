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
}
