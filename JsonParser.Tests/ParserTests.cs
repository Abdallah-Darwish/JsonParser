using JsonParser.Core;

namespace JsonParser.Tests;

public class TestParser
{
    [Fact]
    public void Parse_ComplicatedJson_ParsesFine()
    {
        string src = """
{"1": 2,
            "xyz": [{"abc": [1, 2, 3], 
            
            
            
            "\u0042": 
            
            
            [12, "abc",
            
            
            
             null, false,
             
             
             
              true]}, {     },
              
              
              
              
              
              
              
1.2e-1]}
""";
        var actual = Utility.Print(src);
        string expected = """
{
	"1": 2,
	"xyz": [
		{
			"abc": [
				1,
				2,
				3
			],
			"B": [
				12,
				"abc",
				null,
				false,
				true
			]
		},
		{},
		0.12
	]
}
""";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parse_UnterminatedList_Breaks()
    {
        string src = "[1,2,3";
        var ex = Assert.Throws<ParserException>(() => Utility.Print(src));
        Assert.StartsWith("Expected a token of type RightBracket but got Token", ex.Message);
        Assert.Equal(1, ex.Token.Line);
        Assert.Equal(7, ex.Token.Column);
    }
}
